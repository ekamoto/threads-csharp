﻿using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using ByteBank.View.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            // Pegando o contexto da Thread principal
            var taskSchedulerUI = TaskScheduler.FromCurrentSynchronizationContext();
            BtnProcessar.IsEnabled = false;

            var contas = r_Repositorio.GetContaClientes();
           
            PgsProgresso.Maximum = contas.Count();

            LimparView();

            var inicio = DateTime.Now;

            // var progress = new Progress<String>(str => PgsProgresso.Value++);

            // Essa classe que foi criada implementa praticamente a mesma coisa que o método Progress acima
            // porém dessa forma eu consigo customizar se eu quiser
            var byteBankProgress = new ByteBankProgress<String>(str =>
            PgsProgresso.Value++);

            var resultado = await ConsolidarContas(contas, byteBankProgress);
            
            var fim = DateTime.Now;
            AtualizarView(resultado, fim - inicio);
            BtnProcessar.IsEnabled = true;
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas, IProgress<string> reportadorDeProgresso)
        {
            var tasks = contas.Select(conta =>
                Task.Factory.StartNew(() =>
                {
                    var resultadoConsolidacao = r_Servico.ConsolidarMovimentacao(conta);

                    reportadorDeProgresso.Report(resultadoConsolidacao);

                    return resultadoConsolidacao;
                })
            );

            return await Task.WhenAll(tasks);
        }

        private void AtualizarView(IEnumerable<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }

        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtTempo.Text = null;
            PgsProgresso.Value = 0;
        }
    }
}
