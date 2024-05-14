using System;
using System.Collections.Generic;
using System.Web.UI.DataVisualization.Charting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gammaOpcoes
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Opcao> opcoes = new List<Opcao>();

            //opcoes.Add(new Opcao("TEST4", 32.00, 34.00, 30.00, 8.75, 0.0822));
            //opcoes.Add(new Opcao("ITUB4", 31.94, 30.00, 25.62, 2, 0.06027));
            //opcoes.Add(new Opcao("ITUB4", 31.94, 30.80, 25.62, 2, 0.06027));
            //opcoes.Add(new Opcao("ITUB4", 31.94, 32.90, 25.62, 2, 0.06027));
            opcoes.Add(new Opcao("DOLAR", 5.40, 6.00, 25.00, 2.5, 0.123015873));

            foreach(Opcao opcao in opcoes)
            {
                double r1 = opcao.CalculaBESCall();
                double r2 = opcao.CalculaBESPut();
                double r3 = opcao.CalculaDelta();
                double r4 = opcao.CalculaGamma();
                double r5 = opcao.CalculaTeta();
                double r7 = opcao.CalculaRho();
                double r6 = opcao.CalculaVega();

                opcao.CalculaStress();

                Console.WriteLine("call  :  " + r1);
                Console.WriteLine("put   :  " + r2);
                Console.WriteLine("delta :  " + r3);
                Console.WriteLine("gamma :  " + r4);
                Console.WriteLine("teta  :  " + r5);
                Console.WriteLine("vega  :  " + r6);
                Console.WriteLine("rho   :  " + r7);
                Console.WriteLine("");
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("");
            }
            
            Console.ReadLine();
        }
    }

    class Opcao
    {
        string nomeAtivo;
        double valorAtivo;
        double valorStrike;
        double volatilidadeAtivo;
        double taxaJuros;
        double tempoAteVencimento;

        public Opcao(string nomeAtivo, double valorAtivo, double valorStrike, double volatilidadeAtivo, double taxaJuros, double tempoAteVencimento)
        {
            this.nomeAtivo = nomeAtivo;
            this.valorAtivo = valorAtivo;
            this.valorStrike = valorStrike;
            this.volatilidadeAtivo = volatilidadeAtivo / 100;
            this.taxaJuros = taxaJuros / 100;
            this.tempoAteVencimento = tempoAteVencimento;
        }
        public double CalculaD1()
        {
            double d1;

            d1 = (Math.Log(valorAtivo / valorStrike)+(taxaJuros + Math.Pow(volatilidadeAtivo, 2) / 2) * tempoAteVencimento) / (volatilidadeAtivo * Math.Sqrt(tempoAteVencimento));

            return d1;
        }
        public double CalculaD2()
        {
            double d2;

            double d1 = CalculaD1();

            d2 = d1 - volatilidadeAtivo * Math.Sqrt(tempoAteVencimento);

            return d2;
        }
        public double CalculaBESCall()
        {
            double call;
            double d1 = CalculaD1();
            double d2 = CalculaD2();

            Chart Chart1 = new Chart();
            call = valorAtivo * Chart1.DataManipulator.Statistics.NormalDistribution(d1) - valorStrike * Math.Exp(-taxaJuros * tempoAteVencimento) * Chart1.DataManipulator.Statistics.NormalDistribution(d2);

            return call;
        }
        public double CalculaBESPut()
        {
            double put;
            double d1;
            double d2;

            d1 = CalculaD1();
            d2 = CalculaD2();

            Chart Chart1 = new Chart();
            put = valorStrike * Math.Exp(-taxaJuros * tempoAteVencimento) * Chart1.DataManipulator.Statistics.NormalDistribution(-d2) - valorAtivo * Chart1.DataManipulator.Statistics.NormalDistribution(-d1);

            return put;
        }
        public double CalculaDelta()
        {
            double delta;
            double d1;

            d1 = CalculaD1();

            Chart Chart1 = new Chart();
            delta = Chart1.DataManipulator.Statistics.NormalDistribution(d1) * Math.Exp(0 * tempoAteVencimento);

            return delta;
        }
        public double CalculaGamma()
        {
            double gamma;
            double d1;

            d1 = CalculaD1();

            gamma = NormalPDF(d1) * Math.Exp(-0 * tempoAteVencimento) / (valorAtivo * volatilidadeAtivo * Math.Sqrt(tempoAteVencimento));

            return gamma;
        }
        public double CalculaTeta()
        {
            double teta;
            double d1;
            double d2;

            d1 = CalculaD1();
            d2 = CalculaD2();

            Chart Chart1 = new Chart();
            teta = -valorAtivo * NormalPDF(d1) * volatilidadeAtivo * Math.Exp(-0 * tempoAteVencimento) / (2 * Math.Sqrt(tempoAteVencimento)) + 0 * valorAtivo * Chart1.DataManipulator.Statistics.NormalDistribution(d1) * Math.Exp(-0 * tempoAteVencimento) - taxaJuros * valorStrike * Math.Exp(-taxaJuros * tempoAteVencimento) * Chart1.DataManipulator.Statistics.NormalDistribution(d2);
            
            return teta;
        }
        public double CalculaVega()
        {
            double vega;
            double d1;

            d1 = CalculaD1();

            vega = valorAtivo * Math.Sqrt(tempoAteVencimento) * NormalPDF(d1) * Math.Exp(-0 * tempoAteVencimento);

            return vega;
        }
        public double CalculaRho()
        {
            double rho;
            double d2;

            d2 = CalculaD2();

            Chart Chart1 = new Chart();
            rho = valorStrike * tempoAteVencimento * Math.Exp(-taxaJuros * tempoAteVencimento) * Chart1.DataManipulator.Statistics.NormalDistribution(d2);
            
            return rho;
        }

        public double NormalPDF(double d1)
        {
            double normalPDF;

            normalPDF = 1 / Math.Sqrt(2 * Math.PI) * Math.Exp(-Math.Pow(d1, 2) / 2); ;

            return normalPDF;
        }

        public void CalculaStress()
        {
            List<double> PorcentagemTaxaJuros = CriaListaPorcentagemTaxaJuros();
            List<double> Volatilidades = CriaListaVolatilidades();

            List<double> Resultado = CalculaResultado(PorcentagemTaxaJuros, Volatilidades);

            PrintaObjeto(Resultado);
        }

        public List<double> CriaListaPorcentagemTaxaJuros()
        {
            List<double> PorcentagemTaxaJuros = new List<double>();

            PorcentagemTaxaJuros.Add(1.25); PorcentagemTaxaJuros.Add(1.20); PorcentagemTaxaJuros.Add(1.15);
            PorcentagemTaxaJuros.Add(1.10); PorcentagemTaxaJuros.Add(1.05); PorcentagemTaxaJuros.Add(1);
            PorcentagemTaxaJuros.Add(-1.5); PorcentagemTaxaJuros.Add(-1.10); PorcentagemTaxaJuros.Add(-1.15);
            PorcentagemTaxaJuros.Add(-1.20); PorcentagemTaxaJuros.Add(-1.25);

            return PorcentagemTaxaJuros;
        }

        public List<double> CriaListaVolatilidades()
        {
            List<double> Volatilidades = new List<double>();

            Volatilidades.Add(0.75); Volatilidades.Add(0.80); Volatilidades.Add(0.85); Volatilidades.Add(0.90);
            Volatilidades.Add(0.95); Volatilidades.Add(1); Volatilidades.Add(1.05); Volatilidades.Add(1.10);
            Volatilidades.Add(1.15); Volatilidades.Add(1.20); Volatilidades.Add(1.25);

            return Volatilidades;
        }

        public List<double> CalculaResultado(List<double> PorcentagensTaxaJuros, List<double> Volatilidades)
        {
            List<double> Resultado = new List<double>();

            for (int dia = 0; dia < AnoParaDia(tempoAteVencimento); dia++)
            {
                foreach (double porcentagemTaxaJuros in PorcentagensTaxaJuros)
                {
                    foreach (double volatilidade in Volatilidades)
                    {
                        Opcao opcao = new Opcao(this.nomeAtivo, this.valorAtivo * volatilidade,
                            this.valorStrike, this.volatilidadeAtivo * porcentagemTaxaJuros, this.taxaJuros,
                            0.00273973 * dia);

                        Resultado.Add(opcao.CalculaBESCall());
                    }
                }
            }

            return Resultado;
        }

        public void PrintaObjeto(List<double> objeto)
        {
            int gravadorAux = 0;
            for (var i = 0; i < AnoParaDia(tempoAteVencimento); i++)
            {
                for (var j = 0; j < 11; j++)
                {
                    for (var k = 0; k < 11; k++)
                    {
                        Console.Write(objeto[gravadorAux] + " | ");
                        gravadorAux++;
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        public int AnoParaDia(double anos)
        {
            int dias = Convert.ToInt32(anos * 365);
            return dias;
        }
        public double DiaParaAno(int dias)
        {
            double anos = dias * 365;
            return anos;
        }
    }
}
