using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormFollowOperatorReport : Form
    {
        private readonly string _codigoFJ;
        private readonly FollowUpRepository _followRepo;
        private readonly OperatorRepository _opRepo;

        public FormFollowOperatorReport(
            string codigoFJ,
            FollowUpRepository followRepo,
            OperatorRepository opRepo,
            ShiftRepository shiftRepo,
            SectorRepository sectorRepo,
            FollowUpReasonRepository reasonRepo,
            FollowUpTypeRepository typeRepo,
            EquipmentRepository equipRepo,
            LocalRepository localRepo)
        {
            InitializeComponent();

            _codigoFJ = codigoFJ;
            _followRepo = followRepo;
            _opRepo = opRepo;

            LoadReport();
        }

        private void LoadReport()
        {
            var op = _opRepo.GetByCodigoFJ(_codigoFJ);

            lblTitle.Text = $"{op.NameRomanji} / {op.NameNihongo} ({op.CodigoFJ})";

            var list = _followRepo.GetByOperator(_codigoFJ);

            dgv.DataSource = list.Select(f =>
            {
                var executor = _opRepo.GetByCodigoFJ(f.ExecutorCodigoFJ);
                var witness = f.WitnessCodigoFJ != null
                    ? _opRepo.GetByCodigoFJ(f.WitnessCodigoFJ)
                    : null;

                return new
                {
                    f.Date,
                    f.ShiftName,
                    Operador = $"{op.NameRomanji} / {op.NameNihongo}",
                    Executor = executor != null
                        ? $"{executor.NameRomanji} / {executor.NameNihongo}"
                        : "",
                    Testemunha = witness != null
                        ? $"{witness.NameRomanji} / {witness.NameNihongo}"
                        : "",
                    f.ReasonName,
                    f.TypeName,
                    f.LocalName,
                    f.EquipmentName,
                    f.SectorName,
                    f.Description,
                    f.Guidance
                };
            }).ToList();
        }
    }
}
