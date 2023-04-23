using System.Data.Entity;

namespace Financial.DAL
{
    public class FinancialInitializer : DropCreateDatabaseIfModelChanges<FinancialContext>
	{
		protected override void Seed(FinancialContext context)
		{
			var users = new List<User>
			{
				new User(){Email="bwolf19951212@gmail.com", Username="BWolf", Password="19951212"},
				new User(){Email="naijaspider@gmail.com", Username="Naija", Password="test123"},
			}
			users.ForEach(u => context.Users.add(u));
			context.SaveChanges();
		}
	}
}
