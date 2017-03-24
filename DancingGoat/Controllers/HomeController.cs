﻿using KenticoCloud.Delivery;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DancingGoat.Models;
using KenticoCloud.Personalization;
using KenticoCloud.Personalization.MVC;

namespace DancingGoat.Controllers
{
    public class HomeController : AsyncController
    {
        private readonly DeliveryClient deliveryClient;
        private readonly PersonalizationClient personalizationClient;

        public HomeController()
        {
            deliveryClient = new DeliveryClient(ConfigurationManager.AppSettings["ProjectId"]);

            // Disable personalization when PersonalizationToken is not set
            var personalizationToken = ConfigurationManager.AppSettings["PersonalizationToken"];

            if (!string.IsNullOrWhiteSpace(personalizationToken))
            {
                personalizationClient = new PersonalizationClient(personalizationToken);
            }
        }

        [Route]
        public async Task<ActionResult> Index()
        {
            var response = await deliveryClient.GetItemAsync("home");

            var viewModel = new HomeViewModel
            {
                ContentItem = response.Item,
            };

            // Show promotion banner by default
            var showPromotion = true;

            if (personalizationClient != null)
            {
                // Get User ID of the current visitor
                var visitorUid = Request.GetCurrentPersonalizationUid();
                if (!string.IsNullOrEmpty(visitorUid))
                {
                    // Determine whether the visitor submitted a form
                    var visitorSubmittedForm = await personalizationClient.GetVisitorActionsAsync(visitorUid, ActionTypeEnum.FormSubmit);
                    showPromotion = !visitorSubmittedForm.Activity;
                }
            }

            if (showPromotion)
            {
                viewModel.Header = response.Item.GetModularContent("hero_unit").First(x => x.System.Codename == "home_page_promotion");
            }
            else
            {
                viewModel.Header = response.Item.GetModularContent("hero_unit").First(x => x.System.Codename == "home_page_hero_unit");
            }
            
            return View(viewModel);
        }
    }
}