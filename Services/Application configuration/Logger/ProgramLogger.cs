using GenotypeApplication.Constants;

namespace GenotypeApplication.Services.Application_configuration.Logger
{
    public class ProgramLogger
    {
        private readonly LoggerService _service;
        private readonly SetProcessingStage _stage;

        internal ProgramLogger(LoggerService service, SetProcessingStage stage)
        {
            _service = service;
            _stage = stage;
        }

        public void Info(string message) => _service.Log(_stage, message, LogLevel.Info);
        public void Warning(string message) => _service.Log(_stage, message, LogLevel.Warning);
        public void Error(string message) => _service.Log(_stage, message, LogLevel.Error);

        public void ChangeSet(string? setName)
        {
            _service.SetName = setName;
        }
    }
}
