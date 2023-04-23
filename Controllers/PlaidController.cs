using Microsoft.AspNetCore.Mvc;
using Going.Plaid;
using Going.Plaid.Item;
using Going.Plaid.Entity;
using Going.Plaid.Link;
using Going.Plaid.Transactions;
using Going.Plaid.Accounts;
using Financial.Models;
using Financial.Entities;
using Microsoft.AspNetCore.Identity;
using Financial.DAL;
using Azure.Core;
using System.Diagnostics;
using System.Data;
using System.Threading.Tasks;

namespace Financial.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PlaidController : ControllerBase
    {
        private PlaidClient _client;
        private static readonly Products[] products = { Products.Transactions };
        private static readonly CountryCode[] countryCodes = { CountryCode.Us, CountryCode.Ca };
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        private readonly FinancialContext db;

        public PlaidController(IConfiguration config, UserManager<AppUser> userManager, FinancialContext context)
        {
            db = context;
            _config = config;
            _client = new PlaidClient(
                Going.Plaid.Environment.Sandbox,
                clientId: _config["AppSettings:PlaidClientID"],
                secret: _config["AppSettings:PlaidSecret"]
            );
            _userManager = userManager;
        }

        [HttpPost("create_link_token")]
        public async Task<IActionResult> CreateLinkToken()
        {
            var result =  await _client.LinkTokenCreateAsync(new LinkTokenCreateRequest()
            {
                User = new LinkTokenCreateRequestUser()
                {
                    ClientUserId = "jackbear"
                },
                ClientName = "Financial",
                Products = products,
                CountryCodes = countryCodes,
                Language = Language.English
            });
            return Ok(new
            {
                link_token = result.LinkToken
            });
        }

        [HttpPost("set_access_token")]
        public async Task<IActionResult> SetAccessToken(AccessTokenRequest model)
        {
            var tokenResponse = await _client.ItemPublicTokenExchangeAsync(new ItemPublicTokenExchangeRequest()
            {
                PublicToken = model.public_token
            });
            var accessToken = tokenResponse.AccessToken;
            var itemID = tokenResponse.ItemId;

            var balances = await _client.AccountsBalanceGetAsync(new()
            {
                AccessToken = accessToken,
            });
            var accounts = balances.Accounts;
            var item = balances.Item;

            var _accounts = new List<Entities.Account>();
            var accs = new List<string>();
            foreach (var account in accounts)
            {
                _accounts.Add(new Entities.Account
                {
                    Id = account.AccountId,
                    AvailableBalance = account.Balances.Available,
                    CurrentBalance = account.Balances.Current,
                    LimitBalance = account.Balances.Limit,
                    CurrencyCode = account.Balances.IsoCurrencyCode,
                    Mask = account.Mask,
                    Name = account.Name,
                    OfficialName = account.OfficialName,
                    Subtype = account.Subtype,
                    Type = account.Type,
                    ItemId = item.ItemId
                });
                accs.Add(account.Name);
            }

            var result = await _client.InstitutionsGetByIdAsync(new()
            {
                CountryCodes = countryCodes,
                InstitutionId = balances.Item.InstitutionId!
            });
            var institution = result.Institution;

            var userId = HttpContext.Items["UserId"] as string;

            var newItem = new Entities.Item
            {
                Id = itemID,
                AccessToken = accessToken,
                InstitutionId = institution.InstitutionId,
                InstitutionName = institution.Name,
                InstitutionPrimaryColor = institution.PrimaryColor,
                InstitutionUrl = institution.Url,
                WebhookUrl = item.Webhook,
                UserId = userId,
                Accounts = _accounts
            };

            db.Items.Add(newItem);
            db.SaveChanges();

            return Ok(new
            {
                item_id = itemID,
                institution = institution.Name,
                accounts = accs
            });
        }

        [HttpPost("unlink")]
        public IActionResult Unlink(ItemIdRequest model)
        {
            var item = db.Items.Find(model.item_id);

            if (item == null)
            {
                return BadRequest(new { message = "Item not found." });
            }

            _client.ItemRemoveAsync(new ItemRemoveRequest
            {
                AccessToken = item.AccessToken
            });

            db.Items.Remove(item);
            db.SaveChanges();
            return Ok();
        }

        private async Task<bool> syncFromItem(Entities.Item? item)
        {
            if (item == null) return false;
            var endDate = DateOnly.FromDateTime(DateTime.Today);
            var startDate = endDate.AddYears(-3);
            var lastUpdate = startDate;
            if (item.LastUpdate != null)
            {
                lastUpdate = DateOnly.FromDateTime((DateTime)item.LastUpdate);
            }
            bool finished = false;
            int offset = 0;
            while (!finished)
            {
                var result = await _client.TransactionsGetAsync(new()
                {
                    AccessToken = item.AccessToken,
                    StartDate = lastUpdate,
                    EndDate = endDate,
                    Options = new TransactionsGetRequestOptions
                    {
                        Count = 100,
                        Offset = offset,
                    }
                });
                if (result.Accounts != null)
                {
                    foreach (var account in result.Accounts)
                    {
                        var _account = db.Accounts.Find(account.AccountId);
                        if (_account != null)
                        {
                            _account.AvailableBalance = account.Balances.Available;
                            _account.CurrentBalance = account.Balances.Current;
                            _account.LimitBalance = account.Balances.Limit;
                            _account.CurrencyCode = account.Balances.IsoCurrencyCode;
                            _account.Mask = account.Mask;
                            _account.Name = account.Name;
                            _account.OfficialName = account.OfficialName;
                            _account.Subtype = account.Subtype;
                            _account.Type = account.Type;
                            db.Accounts.Update(_account);
                        }
                    }
                }
                if (result.Transactions != null)
                {
                    foreach (var transaction in result.Transactions)
                    {
                        var _transaction = new Entities.Transaction
                        {
                            Id = transaction.TransactionId,
                            Amount = transaction.Amount,
                            CurrencyCode = transaction.IsoCurrencyCode,
                            Date = transaction.Date.ToDateTime(TimeOnly.MinValue),
                            Name = transaction.Name,
                            // Category = transaction.Category,
                            AccountId = transaction.AccountId,
                            Consolidated = false
                        };
                        try
                        {
                            db.Transactions.Add(_transaction);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                    if (result.Transactions.Count > 0)
                    {
                        offset += result.Transactions.Count;
                    }
                    else
                    {
                        finished = true;
                    }
                }
                else
                {
                    finished = true;
                }
            }
            item.LastUpdate = DateTime.Now;
            db.Items.Update(item);
            return true;
        }

        [HttpPost("sync-transaction")]
        public async Task<IActionResult> TransactionSync(ItemIdRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            if (model.item_id == "all")
            {
                var items = db.Items.Where(i => i.UserId == userId);
                foreach (var item in items)
                {
                    if (await syncFromItem(item) == false)
                    {
                        return BadRequest(new { message = "Item not found." });
                    }
                }
            } else
            {
                var item = db.Items.Find(model.item_id);
                if (await syncFromItem(item) == false)
                {
                    return BadRequest(new { message = "Item not found." });
                }
            }
            db.SaveChanges();
            return Ok();
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> Transactions(ItemIdRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            var _transactions = new List<object>();
            var accounts = new List<Entities.Account>();
            if (model.item_id == "all") {
                var items = db.Items.Where(i => i.UserId == userId);
                foreach (var item in items)
                {
                    accounts.AddRange(db.Accounts.Where(a => a.ItemId == item.Id));
                }
            } else {
                accounts.AddRange(db.Accounts.Where(a => a.ItemId == model.item_id));
            }
            var accIds = new Dictionary<string, string>();
            foreach (var account in accounts)
            {
                accIds[account.Id!] = account.Name!;
            }
            var transactions = db.Transactions
                .Where(t => accIds.Keys.Contains(t.AccountId!))
                .OrderByDescending(t => t.Date);
            foreach (var transaction in transactions)
            {
                _transactions.Add(new
                {
                    account = accIds[transaction.AccountId!],
                    amount = transaction.Amount + " " + transaction.CurrencyCode,
                    name = transaction.Name,
                    date = transaction.Date,
                    consolidated = transaction.Consolidated
                });
            }
            return Ok(_transactions);
        }
    }
}
