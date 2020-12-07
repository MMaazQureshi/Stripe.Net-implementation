using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StripeWeb.Models;
using Stripe;
using System.Text.Json;
using System.IO;

namespace StripeWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public string CreateCustomer()
        {
            StripeConfiguration.ApiKey = "sk_test_51HqZBDLnFgPc5G9BeuyG5lXc32mkG8Hijz01tGKSD6c5ChKpWHSARtJ49yNv75GXY3AbAINw08N8kA1VkvwZtONW00o0QdHxLp";
            var optionsToken = new TokenCreateOptions()
            {
                Card = new TokenCardOptions()
                {
                    Number = "4242424242424242",
                    ExpMonth = 9,
                    ExpYear = 2022,
                    Cvc = "123"
                }
            };
            var serviceToken = new TokenService();
            Token stripeTokenFromApi = serviceToken.CreateAsync(optionsToken).Result;
            var customers = new CustomerService();
            var charges = new ChargeService();

            var customer = customers.Create(new CustomerCreateOptions
            {
                Email = "mmz@gm.com",
                Source = stripeTokenFromApi.Id
            });

            return customer.Id;
          //save customer.Id in database
        }
        //get payment from credit card
        public bool Charge(string customerId)
        {
            StripeConfiguration.ApiKey = "sk_test_51HqZBDLnFgPc5G9BeuyG5lXc32mkG8Hijz01tGKSD6c5ChKpWHSARtJ49yNv75GXY3AbAINw08N8kA1VkvwZtONW00o0QdHxLp";
          
            var charges = new ChargeService();

          
            //var CustomerId = "cus_IRYsZI8vlxpk53";
            var charge = charges.Create(new ChargeCreateOptions
            {
                Amount = 150, //in cents (500cents = 5$)
                Description = "Sample Charge",
                Currency = "usd",
                Customer = customerId
                
            });
            
            return charge.Paid;

        }

       

    
        public string CreateConnectedExpressAccount()
        {
            StripeConfiguration.ApiKey = "sk_test_51HqZBDLnFgPc5G9BeuyG5lXc32mkG8Hijz01tGKSD6c5ChKpWHSARtJ49yNv75GXY3AbAINw08N8kA1VkvwZtONW00o0QdHxLp";

             var options = new AccountCreateOptions
            {
                Type = "express",
            };

            var service = new AccountService();
            var account = service.Create(options);

            var linkOptions = new AccountLinkCreateOptions
            {
                Account = account.Id,
                RefreshUrl = "https://example.com/reauth",
                ReturnUrl = "https://example.com/return",
                Type = "account_onboarding",
            };
            var linkService = new AccountLinkService();
            //user should redirect to this link and complete onboarding process
            var accountLink = linkService.Create(linkOptions);

            return account.Id;//this is connectedStripeAccountId
        }

        public string CreateConnectedCustomAccount()
        {
            StripeConfiguration.ApiKey = "sk_test_51HqZBDLnFgPc5G9BeuyG5lXc32mkG8Hijz01tGKSD6c5ChKpWHSARtJ49yNv75GXY3AbAINw08N8kA1VkvwZtONW00o0QdHxLp";

            var options = new AccountCreateOptions
            {
                Type = "custom",
                Country = "US",
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
                ExternalAccount= new AccountBankAccountOptions()
                {
                    AccountHolderName="abcd",
                    AccountHolderType = "individual",
                    AccountNumber = "000123456789",
                    Country = "US",
                    Currency="usd",
                    RoutingNumber= "110000000"
                },
                BusinessType = "individual",
                Email="test@custom.com"
            };

            var service = new AccountService();
            var account = service.Create(options);

          

            return account.Id;//this is connectedStripeAccountId
        }
        public string UpdateConnectedCustomAccount()
        {
            StripeConfiguration.ApiKey = "sk_test_51HqZBDLnFgPc5G9BeuyG5lXc32mkG8Hijz01tGKSD6c5ChKpWHSARtJ49yNv75GXY3AbAINw08N8kA1VkvwZtONW00o0QdHxLp";

            var options = new AccountUpdateOptions
            {
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    Url = "https://tourtech.com",
                    Name = "example business",
                    SupportEmail="support@example.com",
                    Mcc= "5732"
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
                    AccountHolderName = "abcd",
                    AccountHolderType = "individual",
                    AccountNumber = "000123456789",
                    Country = "US",
                    Currency = "usd",
                    RoutingNumber = "110000000",
                    
                },
                BusinessType = "individual",
                Email = "test@custom.com",
                Individual = new AccountIndividualOptions()
                {
                    FirstName="Maaz",
                    LastName="qureshi",
                    Dob = new DobOptions()  //test dub 1901-01-01
                    {
                        Day=01,
                        Month=01,
                        Year=1901
                    },
                    SsnLast4= "0000",
                    IdNumber = "000000000",
                    Phone = "000 000 0000",

                    Address = new AddressOptions { 
                    State= "Alabama",
                    Country= "us",
                    City= "address_full_match",
                    Line1= "address_full_match",
                    PostalCode= "35004 "
                    },
                    Email="abc@abc.com"
                },
                 TosAcceptance = new AccountTosAcceptanceOptions
                 {
                     Date = DateTime.UtcNow,
                     Ip = "127.0.0.1", // provide request's IP address
                 },
            };

            var service = new AccountService();
            var account = service.Update("acct_1HqwrqQ7aXzmziE6",options);



            return account.Id;//this is connectedStripeAccountId
        }

        //for stripe account to stripe account payment
        public IActionResult TrannsferPayment(string connectedStripeAccountId)
        {
            StripeConfiguration.ApiKey = "sk_test_51HqZBDLnFgPc5G9BeuyG5lXc32mkG8Hijz01tGKSD6c5ChKpWHSARtJ49yNv75GXY3AbAINw08N8kA1VkvwZtONW00o0QdHxLp";
            var options = new TransferCreateOptions
            {
                Amount = 10,
                Currency = "usd",
                //Destination = "acct_1HqgwvCwt0Ou4Dgi", //for test standard account
                //Destination = "acct_1HqwkrQ2LNYZDMiV",  //for test custom account
                Destination = "acct_1HqwrqQ7aXzmziE6",  //for test custom account

                //Destination = connectedStripeAccountId
            };

            var service = new TransferService();
            var Transfer = service.Create(options);
            return Json(new { Transfer.StripeResponse });
        }
        [HttpPost]
        public async Task<IActionResult> WebHookAsync()
        {

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var endpointSecret = "";// _configuration["Stripe:WebHookSecret"];
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);


                if (stripeEvent.Type == Events.AccountUpdated)
                {
                    var account = stripeEvent.Data.Object as Account;
                    //HandleAccountUpdate(account);
                    _logger.LogWarning("Web hook Post in Stripe controller at " + DateTime.Now + " data received " + account);
                    var text = JsonSerializer.Serialize(account);

                }
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }

    }
}
