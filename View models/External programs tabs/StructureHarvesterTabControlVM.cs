using GenotypeApplication.Constants;
using GenotypeApplication.Models.Project;
using GenotypeApplication.View_models.External_programs_tabs;

namespace GenotypeApplication.View_models
{
    public class StructureHarvesterTabControlVM : ExternalProgramTabVMBase
    {
        public StructureHarvesterTabControlVM(WorkflowStateModel workflowStateModel, int coresCount, string fullProjectFolderPath) : base (workflowStateModel, SetProcessingStage.StructureHarvester, coresCount, fullProjectFolderPath)
        {

        }

        protected override void LoadSelectedSetParameters(SetModel? set)
        {
            throw new NotImplementedException();
        }
    }
}
