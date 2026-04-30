using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Interfaces.MVVM;
using GenotypeApplication.Models.CLUMPP;
using GenotypeApplication.Models.Project;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services.Application_configuration.External_program_interaction;
using GenotypeApplication.Services.Application_configuration.Logger;
using GenotypeApplication.Services.MVVM;
using GenotypeApplication.Services.Parsers;
using GenotypeApplication.Services.Set;
using GenotypeApplication.View_models.External_programs_tabs;
using OxyPlot;
using System.Collections.ObjectModel;
using System.IO;

namespace GenotypeApplication.View_models
{
    public class StructureHarvesterTabControlVM : ExternalProgramTabVMBase
    {
        #region Arguments parameters
        private bool _evannoParam;
        private bool _clumppOutputParam;
        #endregion

        #region Graph parameters
        private SetModel? _graphSelectedSetModel;

        private PlotModel _meanLogLKPlotModel = new();
        private PlotModel _firstDiffLKPlotModel = new();
        private PlotModel _secondDiffLKPlotModel = new();
        private PlotModel _deltaKPlotModel = new();

        private readonly EvannoResultsParser _evannoParser;
        private readonly ChartsBuilderService _chartService;
        #endregion

        private readonly StructureHarvesterInteractionService _structureHarvesterInteractionService;

        #region Progress parameters
        private double _structureHarvesterProgress;
        public double StructureHarvesterProgress
        {
            get => _structureHarvesterProgress;
            set { SetField(ref _structureHarvesterProgress, value); }
        }

        private string _structureHarvesterProgressText = "Not started";
        public string StructureHarvesterProgressText
        {
            get => _structureHarvesterProgressText;
            set { SetField(ref _structureHarvesterProgressText, value); }
        }

        private bool _structureHarvesterIsIndeterminate;
        public bool StructureHarvesterIsIndeterminate
        {
            get => _structureHarvesterIsIndeterminate;
            set { SetField(ref _structureHarvesterIsIndeterminate, value); }
        }
        #endregion

        public StructureHarvesterTabControlVM(WorkflowStateModel workflowStateModel, int coresCount, string fullProjectFolderPath, SetConfigurationService setConfigurationService, IDialogService dialogService, IDirectoryService directoryService, IFileService fileService, IMessageService messageService, IValidator<string> pathValidator, IValidator<string> parameterNameValidator, LoggerService loggerService, IValidator<(int kStart, int kEnd, int startLimited, int endLimited)> kRangeValidator) : base(workflowStateModel, SetProcessingStage.StructureHarvester, coresCount, fullProjectFolderPath, setConfigurationService, directoryService, fileService, messageService, dialogService, loggerService, pathValidator, parameterNameValidator, kRangeValidator)
        {

            StartStructureHarvesterAsyncCommand = new AsyncRelayCommand(execute => StartStructureHarvesterAsync(), canExecute => CanStartStructureHarvester());
            LoadChartsAsyncCommand = new AsyncRelayCommand(execute => LoadChartsAsync(), canExecute => CanLoadCharts());

            _structureHarvesterInteractionService = new StructureHarvesterInteractionService(directoryService, fileService, _logger);

            workflowStateModel.StateRefreshed += RebuildGraphComboBoxItems;
            workflowStateModel.SetModelsList.CollectionChanged += (_, _) => RebuildGraphComboBoxItems();
            RebuildGraphComboBoxItems();

            //workflowStateModel.NewSetCreated += ResetProgress;

            _evannoParser = new();
            _chartService = new();
        }

        protected override void ResetProgress()
        {
            UIDispatcherHelper.RunOnUI(() =>
            {
                StructureHarvesterProgress = 0;
                StructureHarvesterProgressText = "Not started";
                StructureHarvesterIsIndeterminate = false;
            });
        }

        #region Arguments parameters properties
        public bool EvannoParam
        {
            get => _evannoParam;
            set { SetField(ref _evannoParam, value); }
        }
        public bool CLUMPPOutputParam
        {
            get => _clumppOutputParam;
            set { SetField(ref _clumppOutputParam, value); }
        }
        #endregion

        #region Graph parameters properties
        public SetModel? GraphSelectedSetModel
        {
            get => _graphSelectedSetModel;
            set => SetField(ref _graphSelectedSetModel, value);
        }
        public ObservableCollection<SetModel> GraphSetModelsComboBoxItems { get; } = new();

        public PlotModel MeanLogLKPlotModel
        {
            get => _meanLogLKPlotModel;
            set { SetField(ref _meanLogLKPlotModel, value); }
        }
        public PlotModel FirstDiffLKPlotModel
        {
            get => _firstDiffLKPlotModel;
            set { SetField(ref _firstDiffLKPlotModel, value); }
        }
        public PlotModel SecondDiffLKPlotModel
        {
            get => _secondDiffLKPlotModel;
            set { SetField(ref _secondDiffLKPlotModel, value); }
        }
        public PlotModel DeltaKPlotModel
        {
            get => _deltaKPlotModel;
            set { SetField(ref _deltaKPlotModel, value); }
        }
        #endregion

        private async void RebuildGraphComboBoxItems()
        {
            var itemsToAdd = new List<SetModel>();

            foreach (SetModel item in FilteredSetModelsList)
            {
                if (item.IsAvailableForStage(SetProcessingStage.CLUMPP))
                {
                    var (evanno, _) = await _structureHarvesterInteractionService.LoadConfiguration(Path.Combine(_fullProjectFolderPath, item.Name));
                    if (evanno) itemsToAdd.Add(item);
                }
            }

            UIDispatcherHelper.RunOnUI(() =>
            {
                GraphSetModelsComboBoxItems.Clear();
                foreach (var item in itemsToAdd)
                {
                    GraphSetModelsComboBoxItems.Add(item);
                }

                if (_graphSelectedSetModel != null &&
                    !GraphSetModelsComboBoxItems.Contains(_graphSelectedSetModel))
                {
                    GraphSelectedSetModel = GraphSetModelsComboBoxItems.FirstOrDefault();
                }
            });
        }

        #region Commands properties
        public AsyncRelayCommand StartStructureHarvesterAsyncCommand { get; }
        public AsyncRelayCommand LoadChartsAsyncCommand { get; }
        #endregion

        protected override async Task LoadSelectedSetParametersAsync(SetModel? set)
        {
            if (set == null || !set.IsStructureHarvesterProcessed)
            {
                ResetProgress();
                return;
            }

            var setName = set.Name;
            var fullSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            try
            {
                var (evanno, clumpp) = await _structureHarvesterInteractionService.LoadConfiguration(fullSetFolderPath);

                EvannoParam = evanno;
                CLUMPPOutputParam = clumpp;

                var popCount = _structureHarvesterInteractionService.GetPopulationsCountFromResults(fullSetFolderPath);
                if (popCount > 0) WorkflowState.SetPredefinedPopCount(popCount);

                if (clumpp || evanno)
                {
                    StructureHarvesterProgress = 100;
                    StructureHarvesterProgressText = $"[{setName}] Completed";
                }
                else
                {
                    StructureHarvesterProgress = 0;
                    StructureHarvesterProgressText = $"[{setName}] Not started";
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading Structure Harvester parameters for set {setName}: {ex.Message}");
            }
        }

        private async Task StartStructureHarvesterAsync()
        {
            if (CurrentSet == null) return;

            var currentSet = CurrentSet;
            var setName = currentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, setName);

            try
            {
                _structureHarvesterInteractionService.PrepareStructureHarvesterDirectory(fullCurrentSetFolderPath);

                var evannoParam = EvannoParam;
                var clumppOutputParam = CLUMPPOutputParam;

                StructureHarvesterIsIndeterminate = true;
                StructureHarvesterProgressText = $"[{setName}] In progress...";
                WorkflowState.MarkUnprocessedAndRefreshStage(currentSet, ProcessingStage);
                IsRunning = true;

                await _structureHarvesterInteractionService.StartExecution(fullCurrentSetFolderPath, evannoParam, clumppOutputParam);

                if (clumppOutputParam)
                    WorkflowState.MarkProcessedAndRefreshStage(currentSet, ProcessingStage);

                await _setConfigurationService.SaveConfigFileAsync(fullCurrentSetFolderPath, currentSet);

                var popCount = _structureHarvesterInteractionService.GetPopulationsCountFromResults(fullCurrentSetFolderPath);
                if (popCount > 0) WorkflowState.SetPredefinedPopCount(popCount);
                StructureHarvesterIsIndeterminate = false;
                StructureHarvesterProgress = 100;
                StructureHarvesterProgressText = $"[{setName}] Completed";
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"An error occurred while running Structure Harvester for set {setName}. {ex.Message} See logs for details.");
                StructureHarvesterProgressText = $"[{setName}] Stopped by error.";
            }
            finally
            {
                IsRunning = false;
            }
        }
        private bool CanStartStructureHarvester()
        {
            return CurrentSet != null &&
                   !_structureHarvesterInteractionService.IsRunning &&
                   (WorkflowState.CanChangeActiveSet != null && WorkflowState.CanChangeActiveSet());
        }

        private async Task LoadChartsAsync()
        {
            try
            {
                if (GraphSelectedSetModel == null) return;

                var selectedSetName = GraphSelectedSetModel.Name;
                var fullSelectedSetFolderPath = Path.Combine(_fullProjectFolderPath, selectedSetName);

                var data = _evannoParser.Parse(fullSelectedSetFolderPath);

                if (data.Count == 0)
                {
                    _messageService.ShowError("No results were found for data processing using the evanno method. Plotting is not possible.");
                    return;
                }

                var (MeanLnPK, LnPrimeK, LnDoublePrimeK, DeltaK) = _chartService.BuildCharts(data);

                MeanLogLKPlotModel = MeanLnPK;
                FirstDiffLKPlotModel = LnPrimeK;
                SecondDiffLKPlotModel = LnDoublePrimeK;
                DeltaKPlotModel = DeltaK;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading charts for set {GraphSelectedSetModel?.Name}: {ex.Message}");
            }
        }
        private bool CanLoadCharts()
        {
            return GraphSelectedSetModel != null;
        }


        protected override async Task LoadSelectedCLUMPPConfigurationAsync(CLUMPPConfigurationModel? configuration)
        {
            return;
        }
    }
}
