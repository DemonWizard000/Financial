import React, { useContext } from "react";

import Endpoint from "../Endpoint";
import Context from "../../Context";
import ProductTypesContainer from "./ProductTypesContainer";
import {
  transactionsCategories,
  authCategories,
  identityCategories,
  balanceCategories,
  investmentsCategories,
  investmentsTransactionsCategories,
  liabilitiesCategories,
  paymentCategories,
  assetsCategories,
  incomePaystubsCategories,
  transferCategories,
  transformAuthData,
  transformTransactionsData,
  transformBalanceData,
  transformInvestmentsData,
  transformInvestmentTransactionsData,
  transformLiabilitiesData,
  transformIdentityData,
  transformPaymentData,
  transformAssetsData,
  transformTransferData,
  transformIncomePaystubsData,
} from "../../dataUtilities";

const Products = () => {
  const { products } = useContext(Context);
  return (
    <ProductTypesContainer productType="Products">
      {products.includes("PaymentInitiation") && (
        <Endpoint
          endpoint="payment"
          name="Payment"
          categories={paymentCategories}
          schema="/payment/get/"
          description="Retrieve information about your latest payment."
          transformData={transformPaymentData}
        />
      )}
      {products.includes("Auth") && (
        <Endpoint
            endpoint="auth"
            name="Auth"
            categories={authCategories}
            schema="/auth/get/"
            description="Retrieve account and routing numbers for checking and savings accounts."
            transformData={transformAuthData}
        />
      )}
      {products.includes("Transactions") && (
        <Endpoint
          endpoint="transactions"
          name="Transactions"
          categories={transactionsCategories}
          schema="/transactions/sync/"
          description="Retrieve transactions or incremental updates for credit and depository accounts."
          transformData={transformTransactionsData}
        />
      )}
      {products.includes("Identity") && (
        <Endpoint
              endpoint="identity"
              name="Identity"
              categories={identityCategories}
              schema="/identity/get/"
              description="Retrieve Identity information on file with the bank. Reduce
              fraud by comparing user-submitted data to validate identity."
              transformData={transformIdentityData}
        />
      )}
      {products.includes("Assets") && (
        <Endpoint
          endpoint="assets"
          name="Assets"
          categories={assetsCategories}
          schema="/assets_report/get/"
          description="Create and retrieve assets information an asset report"
          transformData={transformAssetsData}
        />
      )}
      {!products.includes("PaymentInitiation") && (
          <Endpoint
              endpoint="balance"
              name="Balance"
              categories={balanceCategories}
              schema="/accounts/balance/get/"
              description="Check balances in real time to prevent non-sufficient funds
        fees."
              transformData={transformBalanceData}
          />
      )}
      {products.includes("Investments") && (
        <>
        <Endpoint
          endpoint="holdings"
          name="Investments"
          categories={investmentsCategories}
          schema="/investments/holdings/get/"
          description="Retrieve investment holdings on file with the bank,
        brokerage, or investment institution. Analyze over-exposure
        to market segments."
        transformData={transformInvestmentsData}
      />
      <Endpoint
        endpoint="investments_transactions"
        name="Investments Transactions"
        categories={investmentsTransactionsCategories}
        schema="/investments/transactions/get"
        description="Retrieve investment transactions on file with the bank,
        brokerage, or investments institution."
        transformData={transformInvestmentTransactionsData}
      />
      <Endpoint
        endpoint="liabilities"
        name="Liabilities"
        categories={liabilitiesCategories}
        schema="/liabilities/get"
        description="Retrieve liabilities and various details about an Item with loan or credit accounts."
        transformData={transformLiabilitiesData}
      />
      </>
      )}
      {products.includes("Transfer") && (
        <Endpoint
          endpoint="transfer"
          name="Transfer"
          categories={transferCategories}
          schema="/transfer/get/"
          description="Retrieve information about your latest ACH Transfer."
          transformData={transformTransferData}
        />
      )}
      {products.includes("IncomeVerification") && (
        <Endpoint
          endpoint="/income/verification/paystubs"
          name="Income Paystubs"
          categories={incomePaystubsCategories}
          schema="/income/verification/paystubs"
          description="(Deprecated) Retrieve information from the paystubs used for income verification"
          transformData={transformIncomePaystubsData}
          />
      )}
    </ProductTypesContainer>
  );
};

Products.displayName = "Products";

export default Products;
