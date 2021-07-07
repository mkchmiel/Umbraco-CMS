using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Configuration;


namespace Umbraco.Cms.Infrastructure.ModelsBuilder
{
    public sealed class CodeFirstNotificationHandler : INotificationHandler<UmbracoApplicationStartingNotification>,
        INotificationHandler<UmbracoRequestEndNotification>,
        INotificationHandler<ContentTypeCacheRefresherNotification>,
        INotificationHandler<DataTypeCacheRefresherNotification>
    {

        private static int s_req;

        private readonly ILogger<CodeFirstNotificationHandler> _logger;
        private readonly ModelsBuilderSettings _config;
        private readonly CodeFirstProcessor _codeFirstProcessor;
        private readonly ModelsGenerationError _mbErrors;
        private readonly IMainDom _mainDom;

        public CodeFirstNotificationHandler(ILogger<CodeFirstNotificationHandler> logger,
            IOptions<ModelsBuilderSettings> config,
            CodeFirstProcessor codeFirstProcessor,
            ModelsGenerationError mbErrors,
            IMainDom mainDom)
        {
            _logger = logger;
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            _codeFirstProcessor = codeFirstProcessor;
            _mbErrors = mbErrors;
            _mainDom = mainDom;
        }

        public void Handle(UmbracoApplicationStartingNotification notification) => Process();

        internal bool IsEnabled => _config.ModelsMode == ModelsMode.CodeFirst;
        public void Process()
        {
            //Process should only work for CodeFirst ModelsMode
            if (!IsEnabled)
            {
                return;
            }
            else if (_mainDom.IsMainDom)
            {
                _logger.LogDebug("Code first models processing...");
                _logger.LogInformation("Code first models processing now.");
                _codeFirstProcessor.ProcessCodeFirstModels();
                _mbErrors.Clear();
                _logger.LogInformation("Generated.");
            }
        }

        public void Handle(UmbracoRequestEndNotification notification)
        {
            if (IsEnabled && _mainDom.IsMainDom)
            {
                ProcessModelsIfRequested();
            }
        }

        public void ProcessModelsIfRequested()
        {
            if (Interlocked.Exchange(ref s_req, 0) == 0)
            {
                return;
            }

            // cannot proceed unless we are MainDom
            if (_mainDom.IsMainDom)
            {
                try
                {
                    _logger.LogDebug("Code first models processing...");
                    _logger.LogInformation("Code first models processing now.");
                    _codeFirstProcessor.ProcessCodeFirstModels();
                    _mbErrors.Clear();
                    _logger.LogInformation("Generated.");
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("Timeout, code first models were NOT processed.");
                }
                catch (Exception e)
                {
                    _mbErrors.Report("Failed to process code first models.", e);
                    _logger.LogError("Failed to process code first models.", e);
                }
            }
            else
            {
                // this will only occur if this appdomain was MainDom and it has
                // been released while trying to regenerate models.
                _logger.LogWarning("Cannot process code first models while app is shutting down");
            }
        }

        private void RequestModelsProcessing()
        {
            if (!_mainDom.IsMainDom)
            {
                return;
            }

            _logger.LogDebug("Requested to process code first models.");

            Interlocked.Exchange(ref s_req, 1);
        }

        public void Handle(ContentTypeCacheRefresherNotification notification) => RequestModelsProcessing();
        public void Handle(DataTypeCacheRefresherNotification notification) => RequestModelsProcessing();
    }
}
