using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViralAd.Models.Stripe;
using Stripe.Infrastructure;
using ViralAd.Models.Affiliate;

namespace ViralAd.Stripe
{
    public static class Stripes
    {

        public static string StripeApiKey()
        {
            return StripeConfiguration.ApiKey = Startup.StripeKey;
        }
        #region Payments

        //public static string MakePayment(string amount, string type, string currency)
        //{
        //    StripeApiKey();
        //    var paymentAmount = long.Parse(amount);
        //    var options = new PaymentIntentCreateOptions
        //    {
        //        Amount = paymentAmount * 100,
        //        Currency = currency,//"usd",
        //        PaymentMethodTypes = new List<string>
        //        {
        //           type,// "card",
        //        },
        //    };
        //    var service = new PaymentIntentService();
        //    var payment = service.Create(options);
        //    return payment.Id;
        //}
        public static string MakePayment(long amount, string stripeId)
        {
            try
            {
                StripeApiKey();
                var options = new TransferCreateOptions
                {
                    Amount = amount * 100,
                    Currency = "usd",
                    Destination = stripeId,
                };

                var service = new TransferService();
                var Transfer = service.Create(options);
                return Transfer.Id;
            }
            catch (StripeException ex)
            {

                throw ex;
            }

        }
        public static Charge GetPayment(string stripeId, long amount)
        {
            StripeApiKey();
            var charges = new ChargeService();
            //var CustomerId = "cus_IRYsZI8vlxpk53";
            var charge = charges.Create(new ChargeCreateOptions
            {
                Amount = amount * 100, //in cents (100cents = 1$)
                Description = "Campaign",
                Currency = "usd",
                Customer = stripeId,  //source is already defined for customer
            });

            return charge;

        }
        public static StripeList<PaymentIntent> GetAllPayment(string paymentId)
        {
            var stripeKey = StripeApiKey();
            //var options = new PaymentIntentListOptions
            //{
            //    Limit = 3,
            //};
            var service = new PaymentIntentService();
            StripeList<PaymentIntent> paymentIntents = service.List(
            //options
            );
            return paymentIntents;
        }
        #endregion
        #region Customers
        public static Customer CreateCustomer(string stripeEmail, string stripeToken)
        {
            StripeApiKey();
            var customers = new CustomerService();
            var customer = customers.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                Source = stripeToken
            });

            return customer;
        }
        public static Customer UpdateCustomerCardInformation(StripeCardInfoModel model)
        {
            var stripeKey = StripeApiKey();
            CardCreateNestedOptions card = new CardCreateNestedOptions();
            card.Cvc = model.Cvc;
            card.ExpMonth = model.ExpMonth;
            card.ExpYear = model.ExpYear;
            card.Number = model.Number;
            var token = CreateToken(card);
            var options = new CustomerUpdateOptions
            {
                Source = token.Id
            };
            var service = new CustomerService();
            var customer = service.Update(model.customerId, options);
            return customer;
        }
        #endregion
        #region Tokens
        public static Token CreateToken(CardCreateNestedOptions model)
        {
            StripeApiKey();
            var options = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Number = model.Number,
                    ExpMonth = model.ExpMonth,
                    ExpYear = model.ExpYear,
                    Cvc = model.Cvc,
                },
            };
            var service = new TokenService();
            var token = service.Create(options);
            return token;
        }
        private static Token GetToken(string tokenId)
        {
            var stripeKey = StripeApiKey();

            var service = new TokenService();
            var token = service.Get(tokenId);
            return token;
        }
        #endregion

        #region Products

        public static Product CreateProduct(StripeProductInfoModel model)
        {
            var stripeKey = StripeApiKey();

            var options = new ProductCreateOptions
            {
                Description = model.Description,
                Name = model.Name,
                Active = model.Active,
            };

            var service = new ProductService();
            var product = service.Create(options);

            var priceOption = new PriceCreateOptions
            {
                Product = product.Id,
                UnitAmount = long.Parse(model.Price),
                Currency = "usd",
            };
            var priceService = new PriceService();
            var price = priceService.Create(priceOption);
            return product;
        }

        public static string CreateConnectedAccount(BillingInfoModel model, Models.User.UserInfoModel userDetail)
        {
            //var stripeKey = StripeApiKey();
            var stripeKey = StripeApiKey();

            var options = new AccountCreateOptions
            {
                Type = "custom",
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    Url = "http://viralad.tourtech.co.il/",
                    Name = "affiliate",
                    Mcc = "7311"
                },
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true,
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true,
                    },
                },
                ExternalAccount = new AccountBankAccountOptions()
                {
                    AccountHolderName = model.AccountHolderName,
                    AccountHolderType = "individual",
                    AccountNumber = model.AccountNumber,
                    Country = "US",
                    Currency = "usd",
                    RoutingNumber = model.RoutingNumber,

                },
                BusinessType = "individual",
                Email = userDetail.Email,
                Individual = new AccountIndividualOptions()
                {
                    FirstName = userDetail.FirstName,
                    LastName = userDetail.LastName,
                    Dob = new DobOptions()  //test d0b 1901-01-01
                    {
                        Day = 1, //long.Parse(userDetail.DateOfBirth.Day.ToString()),
                        Month = 1,//long.Parse(userDetail.DateOfBirth.Month.ToString()),
                        Year = 1901 //long.Parse(userDetail.DateOfBirth.Year.ToString())
                    },
                    SsnLast4 = model.SsnLast4,
                    IdNumber = model.IdNumber,
                    Phone = "000 000 0000",//userDetail.Phone,

                    Address = new AddressOptions
                    {
                        State = "Alabama",
                        Country = "us",
                        City = "Moody",
                        Line1 = "address_full_match",
                        PostalCode = "35004"
                    },
                    Email = userDetail.Email
                },
                TosAcceptance = new AccountTosAcceptanceOptions
                {
                    Date = DateTime.UtcNow,
                    Ip = "127.0.0.1", // provide request's IP address
                },

            };

            var service = new AccountService();
            var account = service.Create(options);


            return account.Id;//this is connectedStripeAccountId
        }
        public static string UpdateConnectedAccount(string stripeId, BillingInfoModel model, Models.User.UserInfoModel userDetail)
        {
            //var stripeKey = StripeApiKey();
            //StripeConfiguration.ApiKey = "sk_test_51HqZBDLnFgPc5G9BeuyG5lXc32mkG8Hijz01tGKSD6c5ChKpWHSARtJ49yNv75GXY3AbAINw08N8kA1VkvwZtONW00o0QdHxLp";
            var stripeKey = StripeApiKey();
            var options = new AccountUpdateOptions
            {
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    Url = "http://viralad.tourtech.co.il/",
                    Name = "affiliate",
                    Mcc = "7311"
                },
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true,
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true,
                    },
                },
                ExternalAccount = new AccountBankAccountOptions()
                {
                    AccountHolderName = model.AccountHolderName,
                    AccountHolderType = "individual",
                    AccountNumber = model.AccountNumber,
                    Country = "US",
                    Currency = "usd",
                    RoutingNumber = model.RoutingNumber,

                },
                BusinessType = "individual",
                Email = userDetail.Email,
                Individual = new AccountIndividualOptions()
                {
                    FirstName = userDetail.FirstName,
                    LastName = userDetail.LastName,
                    Dob = new DobOptions()  //test dub 1901-01-01
                    {
                        Day = 1, //long.Parse(userDetail.DateOfBirth.Day.ToString()),
                        Month = 1,//long.Parse(userDetail.DateOfBirth.Month.ToString()),
                        Year = 1901 //long.Parse(userDetail.DateOfBirth.Year.ToString())
                    },
                    SsnLast4 = model.SsnLast4,
                    IdNumber = model.IdNumber,
                    Phone = model.Phone,

                    Address = new AddressOptions
                    {
                        State = "Alabama",
                        Country = "us",
                        City = "Moody",
                        Line1 = "address_full_match",
                        PostalCode = "35004"
                    },
                    Email = model.Email
                },
                TosAcceptance = new AccountTosAcceptanceOptions
                {
                    Date = DateTime.UtcNow,
                    Ip = "127.0.0.1", // provide request's IP address
                },
            };

            var service = new AccountService();
            var account = service.Update(stripeId, options);


            return account.Id;//this is connectedStripeAccountId
        }

        public static object UpdateCustomer(string stripeId, string stripeEmail, string stripeToken)
        {
            StripeApiKey();
            var customers = new CustomerService();

            var customer = customers.Update(stripeId, new CustomerUpdateOptions
            {
                Email = stripeEmail,
                Source = stripeToken
            });

            return customer;
        }
        #endregion

    }
}
