using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence;
using static Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building
{
    /// <summary>
    /// Processes PublishedContentModel classes into the Umbraco DB
    /// </summary>
    public class CodeFirstProcessor
    {
        private readonly UmbracoServices _umbracoService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IShortStringHelper _shortStringHelper;

        public CodeFirstProcessor(UmbracoServices umbracoService, IContentTypeService contentTypeService, IUmbracoMapper umbracoMapper, IShortStringHelper shortStringHelper)
        {
            _umbracoService = umbracoService;
            _contentTypeService = contentTypeService;
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _shortStringHelper = shortStringHelper;
        }

        public void ProcessCodeFirstModels()
        {
            IEnumerable<IContentType> typeModels = _contentTypeService.GetAll();//Doesnt actually get "all" review which service I should be using. That being said this would give the home page and test page

            //Get all classes that inherited PublishedContentModel
            IEnumerable<System.Reflection.Assembly> filteredAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(w => w.GetTypes().Where(w2 => w2.BaseType == typeof(PublishedContentModel)).Any());
            IEnumerable<Type> codeModels = filteredAssemblies.SelectMany(s => s.GetTypes()).Where(w => w.BaseType == typeof(PublishedContentModel));

            //Id and id are not the same thing, review how this is created
            foreach (Type model in codeModels)
            {
                var publishedModelAttribute = (PublishedModelAttribute)model.GetCustomAttributes(typeof(PublishedModelAttribute), false).FirstOrDefault();
                var alias = publishedModelAttribute != null ? publishedModelAttribute.ContentTypeAlias : model.Name;

                IContentType dbModel = typeModels.Where(a => a.Alias == alias).FirstOrDefault();
                ContentType contentTypeModel = CreateContentTypeModelFromClass(model);

                if (dbModel != null)
                {
                    CompareDBToCode(contentTypeModel, dbModel);
                }
                else
                {
                    //Validate the proper way to save and update, probably following sort of how front end does it but it may be weird trying to map class to model e.g. the below where it expects a IContentType
                    //Probably set the defaults here or something
                    //How do we know if the model should be at root level???
                    contentTypeModel.PropertyGroups = new PropertyGroupCollection();
                    _contentTypeService.Save(contentTypeModel);
                }


            }
        }

        public void CompareDBToCode(ContentType contentTypeModel, IContentType dbModel)
        {
            var differenceFound = false;

            if(contentTypeModel.Alias != dbModel.Alias)
            {
                dbModel.Alias = contentTypeModel.Alias;
                differenceFound = true;
            }

            //Set the expected values from dbModel in codeModel
            //Ensure not to overwrite anything
            //Right now it seeks that

            if(differenceFound)
            {
                _contentTypeService.Save(dbModel);
            }
        }



        public ContentType CreateContentTypeModelFromClass(Type codeModel)
        {
            var publishedModelAttribute = (PublishedModelAttribute)codeModel.GetCustomAttributes(typeof(PublishedModelAttribute), false).FirstOrDefault();

            var test = new PropertyGroupCollection();
            var test2 = new PropertyGroup(false);

            var parentId = -1;
            var contentTypeModel = new ContentType(_shortStringHelper, parentId)
            {
                Alias = publishedModelAttribute != null ? publishedModelAttribute.ContentTypeAlias : codeModel.Name,
                Description = publishedModelAttribute != null ? publishedModelAttribute.ContentTypeDescription : string.Empty,
                Icon = publishedModelAttribute != null ? publishedModelAttribute.ContentTypeIcon : Icons.Content,
                Thumbnail = publishedModelAttribute != null ? publishedModelAttribute.ContentTypeThumbnail : "folder.png",
                AllowedAsRoot = publishedModelAttribute != null ? publishedModelAttribute.AllowedAsRoot : false,
                Name = publishedModelAttribute != null ? publishedModelAttribute.ContentTypeName : codeModel.Name,
                PropertyGroups = test
            };

            return contentTypeModel;
        }
    }
}
