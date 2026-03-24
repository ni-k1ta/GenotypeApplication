using GenotypeApplication.Constants;
using GenotypeApplication.MVVM.Infrastructure;

namespace GenotypeApplication.Models.Project
{
    public class SetModel : ViewModelBase
    {
        private string _name = string.Empty;

        private bool _isStructureProcessed = false;
        private bool _isStructureHarvesterProcessed = false;
        private bool _isCLUMPPProcessed = false;
        private bool _isDistructProcessed = false;

        private bool _isCurrent;

        public string Name
        {
            get => _name;
            set { SetField(ref _name, value); }
        }

        public bool IsStructureProcessed
        {
            get => _isStructureProcessed;
            set { SetField(ref _isStructureProcessed, value); }
        }
        public bool IsStructureHarvesterProcessed
        {
            get => _isStructureHarvesterProcessed;
            set { SetField(ref _isStructureHarvesterProcessed, value); }
        }
        public bool IsCLUMPPProcessed
        {
            get => _isCLUMPPProcessed;
            set { SetField(ref _isCLUMPPProcessed, value); }
        }
        public bool IsDistructProcessed
        {
            get => _isDistructProcessed;
            set { SetField(ref _isDistructProcessed, value); }
        }

        public bool IsCurrent
        {
            get => _isCurrent;
            set { SetField(ref _isCurrent, value); }
        }

        public bool IsProcessedForStage(SetProcessingStage stage)
        {
            return stage switch
            {
                SetProcessingStage.Structure => IsStructureProcessed,
                SetProcessingStage.StructureHarvester => IsStructureHarvesterProcessed,
                SetProcessingStage.CLUMPP => IsCLUMPPProcessed,
                SetProcessingStage.Distruct => IsDistructProcessed,
                _ => false
            };
        }
        public void MarkAsProcessedForStage(SetProcessingStage stage)
        {
            switch (stage)
            {
                case SetProcessingStage.Structure:
                    IsStructureProcessed = true;
                    break;
                case SetProcessingStage.StructureHarvester:
                    IsStructureHarvesterProcessed = true;
                    break;
                case SetProcessingStage.CLUMPP:
                    IsCLUMPPProcessed = true;
                    break;
                case SetProcessingStage.Distruct:
                    IsDistructProcessed = true;
                    break;
            }
        }
        public bool IsAvailableForStage(SetProcessingStage stage)
        {
            return stage switch
            {
                SetProcessingStage.Structure => true,
                SetProcessingStage.StructureHarvester => IsStructureProcessed,
                SetProcessingStage.CLUMPP => IsStructureHarvesterProcessed,
                SetProcessingStage.Distruct => IsCLUMPPProcessed,
                _ => false
            };
        }
    }
}
