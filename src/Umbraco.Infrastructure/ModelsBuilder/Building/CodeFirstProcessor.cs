using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building
{
    /// <summary>
    /// 
    /// </summary>
    public class CodeFirstProcessor
    {
        private readonly UmbracoServices _umbracoService;
        private readonly IContentTypeService _contentTypeService;

        public CodeFirstProcessor(UmbracoServices umbracoService, IContentTypeService contentTypeService)
        {
            _umbracoService = umbracoService;
            _contentTypeService = contentTypeService;
        }

        public void ProcessCodeFirstModels()
        {
            //3 Cases to handle
            //1. New class not in DB
            //2. Class in DB that has changed
            //3. Class in DB does not exist in code for deletion???
            System.Collections.Generic.IList<TypeModel> typeModels = _umbracoService.GetAllTypes();

            //Get all assemblies associated with current deploy, then get only those assemblies that have classes that inherited PublishedContentModel
            System.Reflection.Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<System.Reflection.Assembly> filteredAssemblies = allAssemblies.Where(w => w.GetTypes().Where(w2 => w2.IsSubclassOf(typeof(PublishedContentModel)) && !w2.IsAbstract).Any());

            foreach(System.Reflection.Assembly assembly in filteredAssemblies)
            {
                foreach(Type codeModel in assembly.GetTypes())
                {
                    TypeModel dbModel = typeModels.Where(a => a.Name == codeModel.Name).FirstOrDefault();
                    if (dbModel != null)
                    {
                        CompareDBToCode(codeModel, dbModel);
                    }
                    else
                    {
                        //Validate the proper way to save and update, probably following sort of how front end does it but it may be weird trying to map class to model e.g. the below where it expects a IContentType
                        //_contentTypeService.Save(codeModel);
                    }

                }
            }
        }

        public void CompareDBToCode(Type codeModel, TypeModel dbModel)
        {

        }

        public void UpdateDBModel(Type codeModel)
        {

        }
    }
}
