using GenotypeApplication.Constants;
using GenotypeApplication.Models.Project;
using GenotypeApplication.View_models.External_programs_tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.View_models
{
    public class DistructTabControlVM : ExternalProgramTabVMBase
    {
        public DistructTabControlVM(WorkflowStateModel workflowState, int coresCount, string fullProjectFolderPath) : base(workflowState, SetProcessingStage.Distruct, coresCount, fullProjectFolderPath)
        {
        }
        protected override void LoadSelectedSetParameters(SetModel? set)
        {
            throw new NotImplementedException();
        }
    }
}
