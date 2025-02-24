using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace WebApplication1.Controllers
{
    public class MicroondasController : Controller
    {
        private static readonly string[] status = new[] { "Espera", "Andamento", "Pausado" };

        private readonly IWebHostEnvironment _env;

        public MicroondasController(IWebHostEnvironment env)
        {
            _env = env;
        }



        public IActionResult Index()
        {
            ViewBag.Painel = "";
            ViewBag.Tempo = 0;
            ViewBag.Potencia = "0";
            ViewBag.Estado = "Espera";
            ViewBag.Execao = 0;

            ViewBag.Programas = LoadProgramasJson("presets.json", "Alimentos");

            return View();
        }

        [HttpPost]
        public IActionResult Executar(int potencia, string tempo, string estado, int excecao)
        {
            ViewBag.Programas = LoadProgramasJson("presets.json", "Alimentos");
            ViewBag.Execao = excecao;
            if (!status.Contains(estado, StringComparer.OrdinalIgnoreCase) && estado != null && estado != "")
            {
                ModelState.AddModelError("Estado", "O estado não pode ser diferente de Espera, Andamento ou Pausado");
                return View("Index");
            }
            else
            {
                if (estado == "Espera" || estado == "Pausado")
                {
                    Iniciar(potencia, tempo, estado, excecao);
                }

                if (estado == "Andamento" && excecao == 0)
                {
                    AumentaTempo(potencia, tempo, estado);
                }

                if (estado == "Andamento" && excecao == 1)
                {

                    if (int.TryParse(tempo.Substring(0, 2), out int minutos) && int.TryParse(tempo.Substring(3, 2), out int segundos))
                    {
                        TimeSpan valorTempo = new TimeSpan(0, minutos, segundos);

                        string Painel = "";

                        for (int i = 0; i < valorTempo.TotalSeconds; i++)
                        {
                            for (int j = 0; j < potencia; j++)
                            {
                                Painel += ".";
                            }

                            Painel += " ";
                        }

                        ViewBag.Painel = Painel;
                        ViewBag.Tempo = valorTempo.TotalSeconds;
                    }
                   
                    ViewBag.Potencia = potencia;
                    ViewBag.Estado = estado;

                    ModelState.AddModelError("Tempo", "O tempo não pode ser incrementado para programas definidos");
                }

            }

            return View("Index");
        }

        public IActionResult Iniciar(int potencia, string tempo, string estado, int? excecao)
        {
            if (tempo != null || tempo != "")
            {
                tempo = tempo.Trim();
            }
            else
            {
                ModelState.AddModelError("Tempo", "Formato de tempo inválido. Use MM:SS.");
            }

            if ((tempo == null || tempo == "" || tempo == "00:00") && excecao == 0)
            {
                tempo = "00:30";
            }


            if (potencia == 0)
            {
                potencia = 10;
            }

            string Painel = "";

            if (tempo.Length == 5 && tempo[2] == ':')
            {

                if (int.TryParse(tempo.Substring(0, 2), out int minutos) && int.TryParse(tempo.Substring(3, 2), out int segundos))
                {
                    TimeSpan valorTempo = new TimeSpan(0, minutos, segundos);

                    if ((valorTempo.TotalSeconds > 120 || valorTempo.TotalSeconds < 0) && excecao == 0)
                    {
                        ModelState.AddModelError("Tempo", "O tempo precisa ser entre 00:01 e 02:00.");
                    }
                    else
                    {
                        for (int i = 0; i < valorTempo.TotalSeconds; i++)
                        {
                            for (int j = 0; j < potencia; j++)
                            {
                                Painel += ".";
                            }

                            Painel += " ";
                        }

                        ViewBag.Tempo = valorTempo.TotalSeconds;
                    }


                }
                else
                {
                    ModelState.AddModelError("Tempo", "Formato de tempo inválido. Use MM:SS.");
                }
            }
            else
            {
                ModelState.AddModelError("Tempo", "Formato de tempo inválido. Use MM:SS.");
            }

            if (potencia < 1 || potencia > 10)
            {
                ModelState.AddModelError("Potencia", "A potencia deve ser entre 1 e 10.");
            }

            if (!ModelState.IsValid)
            {
                return View("Index");
            }

            estado = "Andamento";

            ViewBag.Painel = Painel;
            ViewBag.Potencia = potencia;
            ViewBag.Estado = estado;

            return View("Index");
        }

        public IActionResult AumentaTempo(int potencia, string tempo, string estado)
        {

            if (tempo != null || tempo == "")
            {
                tempo = tempo.Trim();
            }

            if (potencia == 0)
            {
                potencia = 10;
            }

            string Painel = "";

            if (tempo.Length == 5 && tempo[2] == ':')
            {

                if (int.TryParse(tempo.Substring(0, 2), out int minutos) && int.TryParse(tempo.Substring(3, 2), out int segundos))
                {
                    //TimeSpan tempoValue = new TimeSpan(0, minutes, seconds + 30);
                    segundos = segundos + 30;

                    int totalSegundos = minutos * 60 + segundos;

                    if (totalSegundos > 60 )
                    {
                        minutos = (int)Math.Floor( (decimal)(totalSegundos/60));
                        segundos = totalSegundos % 60;

                    }
                    if(totalSegundos > 120)
                    {
                        minutos = 2;
                        segundos = 0;
                    }

                    TimeSpan valorTempo = new TimeSpan(0, minutos, segundos);

                    for (int i = 0; i < valorTempo.TotalSeconds; i++)
                    {
                        for (int j = 0; j < potencia; j++)
                        {
                            Painel += ".";
                        }

                        Painel += " ";
                    }

                     ViewBag.Tempo = valorTempo.TotalSeconds;

                }
                else
                {
                    ModelState.AddModelError("Tempo", "Formato de tempo inválido. Use MM:SS.");
                }
            }
            else
            {
                ModelState.AddModelError("Tempo", "Formato de tempo inválido. Use MM:SS.");
            }

            if (potencia < 0 || potencia > 10)
            {
                ModelState.AddModelError("Potencia", "A potencia deve ser entre 0 e 10.");
            }

            if (!ModelState.IsValid)
            {
                return View("Index");
            }

            estado = "Andamento";

            ViewBag.Painel = Painel;
            ViewBag.Potencia = potencia;
            ViewBag.Estado = estado;

            return View("Index");
        }

        public IActionResult Pausar(int potencia, string tempo, string estado, int excecao)
        {
            ViewBag.Programas = LoadProgramasJson("presets.json", "Alimentos");
            ViewBag.Execao = excecao;

            if (tempo != null || tempo != "")
            {
                tempo = tempo.Trim();
            }
            else
            {
                ModelState.AddModelError("Tempo", "Formato de tempo inválido. Use MM:SS.");
            }


            if (potencia == 0)
            {
                potencia = 10;
            }

            string Painel = "";

            if(estado == "Andamento")
            {
                if (tempo.Length == 5 && tempo[2] == ':')
                {

                    if (int.TryParse(tempo.Substring(0, 2), out int minutos) && int.TryParse(tempo.Substring(3, 2), out int segundos))
                    {
                        TimeSpan valorTempo = new TimeSpan(0, minutos, segundos);

                        if ((valorTempo.TotalSeconds > 120 || valorTempo.TotalSeconds < 0) && excecao == 0)
                        {
                            ModelState.AddModelError("Tempo", "O tempo precisa ser entre 00:01 e 02:00.");
                        }
                        else
                        {
                            for (int i = 0; i < valorTempo.TotalSeconds; i++)
                            {
                                for (int j = 0; j < potencia; j++)
                                {
                                    Painel += ".";
                                }

                                Painel += " ";
                            }

                            ViewBag.Tempo = valorTempo.TotalSeconds;
                        }


                    }
                    else
                    {
                        ModelState.AddModelError("Tempo", "Formato de tempo inválido. Use MM:SS.");
                    }
                }
                else
                {
                    ModelState.AddModelError("Tempo", "Formato de tempo inválido. Use MM:SS.");
                }

                if (potencia < 1 || potencia > 10)
                {
                    ModelState.AddModelError("Potencia", "A potencia deve ser entre 1 e 10.");
                }

                if (!ModelState.IsValid)
                {
                    return View("Index");
                }

                estado = "Pausado";
            }
            else
            {
                potencia = 0;
                estado = "Espera";
                ViewBag.Tempo = "00:00";
            }

            ViewBag.Painel = Painel;
            ViewBag.Potencia = potencia;
            ViewBag.Estado = estado;

            return View("Index");
        }

        private string LoadProgramasJson(string jsonFileName, string key)
        {
            // Construct the file path
            var filePath = Path.Combine(_env.WebRootPath, $"../Data/presets/{jsonFileName}");

            // Read the JSON file
            var jsonData = System.IO.File.ReadAllText(filePath);

            // Deserialize the JSON data
            var programas = JsonConvert.DeserializeObject<Dictionary<string, List<Programa>>>(jsonData);

            // Serialize the specific key's data to JSON
            return JsonConvert.SerializeObject(programas[key]);
        }

        /*public IActionResult RetornaErroExecucao(int potencia, string tempo, string estado)
        {


        }*/
    }
}
