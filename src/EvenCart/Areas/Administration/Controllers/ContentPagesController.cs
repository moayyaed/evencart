﻿using System;
using System.Linq;
using EvenCart.Areas.Administration.Extensions;
using EvenCart.Areas.Administration.Models.Pages;
using EvenCart.Areas.Administration.Models.Users;
using EvenCart.Data.Constants;
using EvenCart.Data.Entity.Pages;
using EvenCart.Data.Extensions;
using EvenCart.Services.Pages;
using EvenCart.Services.Serializers;
using EvenCart.Infrastructure;
using EvenCart.Infrastructure.Mvc;
using EvenCart.Infrastructure.Mvc.Attributes;
using EvenCart.Infrastructure.Mvc.ModelFactories;
using EvenCart.Infrastructure.Routing;
using EvenCart.Infrastructure.Security.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace EvenCart.Areas.Administration.Controllers
{
    public class ContentPagesController : FoundationAdminController
    {
        private readonly IContentPageService _contentPageService;
        private readonly IModelMapper _modelMapper;
        private readonly IDataSerializer _dataSerializer;
        private readonly ISeoMetaService _seoMetaService;
        public ContentPagesController(IContentPageService contentPageService, IModelMapper modelMapper, IDataSerializer dataSerializer, ISeoMetaService seoMetaService)
        {
            _contentPageService = contentPageService;
            _modelMapper = modelMapper;
            _dataSerializer = dataSerializer;
            _seoMetaService = seoMetaService;
        }

        [DualGet("", Name = AdminRouteNames.ContentPagesList)]
        [CapabilityRequired(CapabilitySystemNames.ViewContentPages)]
        public IActionResult ContentPagesList(ContentPageSearchModel parameters)
        {
            var current = parameters?.Current ?? 1;
            var rowCount = parameters?.RowCount ?? 15;
            var contentPages =
                _contentPageService.GetContentPages(out int totalResults, parameters?.SearchPhrase, current, rowCount);

            var contentPageModels = contentPages.Select(PrepareModel).Select(x =>
            {
                x.Content = ""; //no need to send content on list page...will save some bandwidth
                return x;
            });
            return R.Success.WithGridResponse(totalResults, current, rowCount)
                .With("contentPages", () => contentPageModels, () => _dataSerializer.Serialize(contentPageModels))
                .WithParams(parameters)
                .Result;
        }

        [DualGet("{contentPageId}", Name = AdminRouteNames.GetContentPage)]
        [CapabilityRequired(CapabilitySystemNames.EditContentPage)]
        public IActionResult ContentPageEditor(int contentPageId)
        {
            var contentPage = contentPageId > 0 ? _contentPageService.Get(contentPageId) : new ContentPage();
            if (contentPage == null)
                return NotFound();
            var model = PrepareModel(contentPage);
            return R.Success.With("contentPage", model)
                .Result;
        }

        [DualPost("", Name = AdminRouteNames.SaveContentPage, OnlyApi = true)]
        [CapabilityRequired(CapabilitySystemNames.EditContentPage)]
        [ValidateModelState(ModelType = typeof(ContentPageModel))]
        public IActionResult SaveContentPage(ContentPageModel model)
        {
            var contentPage = model.Id > 0 ? _contentPageService.Get(model.Id) : new ContentPage();
            if (contentPage == null)
                return NotFound();
            if (model.Published && (model.SeoMeta?.Slug.IsNullEmptyOrWhiteSpace() ?? true))
            {
                if (model.Id > 0)
                    return R.Fail.With("error", T("Can't publish page without slug")).Result;
            }
            _modelMapper.Map(model, contentPage, nameof(ContentPage.CreatedOn), nameof(ContentPage.PublishedOn), nameof(ContentPage.UserId));
            if (contentPage.Id == 0)
            {
                contentPage.CreatedOn = DateTime.UtcNow;
                contentPage.PublishedOn = model.PublishedOn == default(DateTime) ? DateTime.UtcNow : model.PublishedOn;
                contentPage.UserId = ApplicationEngine.CurrentUser.Id;
            }
            contentPage.UpdatedOn = DateTime.UtcNow;
            _contentPageService.InsertOrUpdate(contentPage);

            //update the seometa
            _seoMetaService.UpdateSeoMetaForEntity(contentPage, model.SeoMeta);
            return R.Success.Result;
        }

        [DualPost("delete", Name = AdminRouteNames.DeleteContentPage, OnlyApi = true)]
        [CapabilityRequired(CapabilitySystemNames.DeleteContentPage)]
        public IActionResult DeleteContentPage(int contentPageId)
        {
            var contentPage = _contentPageService.Get(contentPageId);
            if (contentPage == null)
                return NotFound();
            _contentPageService.Delete(contentPage);
            return R.Success.Result;
        }

        #region Helpers

        private ContentPageModel PrepareModel(ContentPage page)
        {
            var model = _modelMapper.Map<ContentPageModel>(page);
            model.SeoMeta = _modelMapper.Map<SeoMetaModel>(page.SeoMeta);
            model.User = _modelMapper.Map<UserMiniModel>(page.User);
            return model;
        }
        #endregion

    }
}