using GenotypeApplication.Constants;
using GenotypeApplication.Interfaces;
using GenotypeApplication.Models.Project;
using GenotypeApplication.MVVM.Infrastructure;
using GenotypeApplication.Services.Application_configuration.External_program_interaction;
using GenotypeApplication.Services.MVVM;
using GenotypeApplication.Services.Parsers;
using GenotypeApplication.View_models.External_programs_tabs;
using OxyPlot;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

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

        public StructureHarvesterTabControlVM(WorkflowStateModel workflowStateModel, int coresCount, string fullProjectFolderPath, IDialogService dialogService, IDirectoryService directoryService, IFileService fileService, IMessageService messageService) : base(workflowStateModel, SetProcessingStage.StructureHarvester, coresCount, fullProjectFolderPath, directoryService, fileService, messageService, dialogService)
        {

            StartStructureHarvesterAsyncCommand = new AsyncRelayCommand(execute => StartStructureHarvesterAsync(), canExecute => CanStartStructureHarvester());
            LoadChartsAsyncCommand = new AsyncRelayCommand(execute => LoadChartsAsync(), canExecute => CanLoadCharts());

            _structureHarvesterInteractionService = new StructureHarvesterInteractionService(directoryService, fileService);

            workflowStateModel.StateRefreshed += RebuildGraphComboBoxItems;
            workflowStateModel.SetModelsList.CollectionChanged += (_, _) => RebuildGraphComboBoxItems();
            RebuildGraphComboBoxItems();

            _evannoParser = new();
            _chartService = new();
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

        private void RebuildGraphComboBoxItems()
        {
            UIDispatcherHelper.RunOnUI(() =>
            {
                GraphSetModelsComboBoxItems.Clear();
                foreach (SetModel item in WorkflowState.SetModelsList)
                {
                    if (item.IsProcessedForStage(ProcessingStage))
                        GraphSetModelsComboBoxItems.Add(item);
                }

                //если выбранный Set пропал из списка — сбросить
                if (_graphSelectedSetModel != null &&
                    !GraphSetModelsComboBoxItems.Contains(_graphSelectedSetModel))
                {
                    GraphSelectedSetModel = null;
                }
            });
        }

        #region Commands properties
        public AsyncRelayCommand StartStructureHarvesterAsyncCommand { get; }
        public AsyncRelayCommand LoadChartsAsyncCommand { get; }
        #endregion

        protected override void LoadSelectedSetParameters(SetModel? set)
        {

        }

        private async Task StartStructureHarvesterAsync()
        {
            if (CurrentSet == null) return;

            var currentSetName = CurrentSet.Name;
            var fullCurrentSetFolderPath = Path.Combine(_fullProjectFolderPath, currentSetName);

            try
            {
                _structureHarvesterInteractionService.PrepareStructureHarvesterDirectory(fullCurrentSetFolderPath);

                var evannoParam = EvannoParam;
                var clumppOutputParam = CLUMPPOutputParam;

                await _structureHarvesterInteractionService.StartExecution(fullCurrentSetFolderPath, evannoParam, clumppOutputParam);

                WorkflowState.MarkProcessedAndRefreshStage(CurrentSet, ProcessingStage);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private bool CanStartStructureHarvester()
        {
            return CurrentSet != null && !_structureHarvesterInteractionService.IsRunning;
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
                    //todo
                    return;
                }

                var (MeanLnPK, LnPrimeK, LnDoublePrimeK, DeltaK) = _chartService.BuildCharts(data);

                MeanLogLKPlotModel = MeanLnPK;
                FirstDiffLKPlotModel = LnPrimeK;
                SecondDiffLKPlotModel = LnDoublePrimeK;
                DeltaKPlotModel = DeltaK;
            }
            catch (Exception)
            {
                //todo
            }
        }
        private bool CanLoadCharts()
        {
            return GraphSelectedSetModel != null;
        }
    }
}
