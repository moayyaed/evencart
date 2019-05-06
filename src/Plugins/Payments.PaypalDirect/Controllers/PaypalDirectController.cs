﻿using System;
using EvenCart.Infrastructure.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Payments.PaypalDirect.Models;

namespace Payments.PaypalDirect.Controllers
{
    [Route("paypaldirect")]
    public class PaypalDirectController : FoundationController
    {
        [HttpGet("payment-info", Name = PaypalDirectConfig.PaymentHandlerComponentRouteName)]
        public IActionResult PaymentInfoDisplayPage()
        {
            var model = new PaymentInfoModel();
            var baseYear = DateTime.UtcNow.Year;
            for (var i = 1; i < 13; i++)
            {
                var value = i.ToString();
                model.AvailableMonths.Add(new SelectListItem(value, value));
            }
            //50 years from now
            for (var i = 0; i < 51; i++)
            {
                var value = (baseYear + i).ToString();
                model.AvailableYears.Add(new SelectListItem(value, value));
            }
            model.Month = DateTime.UtcNow.Month;
            model.Year = DateTime.UtcNow.Year;
            return R.With("paymentInfo", model).Result;
        }
    }
}