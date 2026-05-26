using GenotypeApplication.Constants;

namespace GenotypeApplication.Services.Application_configuration.Logger
{
    public class ProgramLogger
    {
        private readonly LoggerService _service;
        private readonly SetProcessingStage _stage;
        private readonly string? _unitKey;

        internal ProgramLogger(LoggerService service, SetProcessingStage stage, string? unitKey)
        {
            _service = service;
            _stage = stage;
            _unitKey = unitKey;
        }

        public void Info(string message) => _service.Log(_stage, message, LogLevel.Info, _unitKey);
        public void Warning(string message) => _service.Log(_stage, message, LogLevel.Warning, _unitKey);
        public void Error(string message) => _service.Log(_stage, message, LogLevel.Error, _unitKey);

        public void ChangeSet(string? setName)
        {
            _service.SetName = setName;
        }

        // создаёт дочерний логгер для конкретного юнита (k, i)
        public ProgramLogger ForUnit(string unitKey)
            => new ProgramLogger(_service, _stage, unitKey);

        // собирает буферы дочерних логгеров в финальный файл в нужном порядке
        public void AssembleUnitLogs(IEnumerable<string> orderedUnitKeys)
            => _service.AssembleUnitLogs(_stage, orderedUnitKeys);
        public void RecoverOrphanedUnits() => _service.RecoverOrphanedUnits(_stage);
    }
}
