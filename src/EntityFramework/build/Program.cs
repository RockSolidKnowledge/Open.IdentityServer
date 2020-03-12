using System;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    class Program
    {
        private const string Prefix = "EntityFramework";
        private const bool RequireTests = false;

        private const string ArtifactsDir = "artifacts";
        private const string Build = "build";
        private const string Test = "test";
        private const string Pack = "pack";
        
        static void Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect
            };
            
            var sign = app.Option<(bool hasValue, int theValue)>("--sign", "Sign binaries and nuget package", CommandOptionType.SingleOrNoValue);

            CleanArtifacts();

            app.OnExecute(() =>
            {
                Target(Build, () => 
                {
                    Run("dotnet", $"build -c Release", echoPrefix: Prefix);

                    if (sign.HasValue())
                    {
                        Sign("*.dll", "./src/bin/release");
                    }
                });

                Target(Test, DependsOn(Build), () => 
                {
                    Run("dotnet", $"test -c Release --no-build", echoPrefix: Prefix);
                        
                });
                
                Target(Pack, DependsOn(Build), () => 
                {
                    var project = Directory.GetFiles("./src", "*.csproj", SearchOption.TopDirectoryOnly).OrderBy(_ => _).First();

                    Run("dotnet", $"pack {project} -c Release -o ./{ArtifactsDir} --no-build", echoPrefix: Prefix);
                    
                    if (sign.HasValue())
                    {
                        Sign("*.nupkg", $"./{ArtifactsDir}");
                    }

                    CopyArtifacts();
                });

                Target("quick", () => 
                {
                    var project = Directory.GetFiles("./src", "*.csproj", SearchOption.TopDirectoryOnly).OrderBy(_ => _).First();

                    Run("dotnet", $"pack {project} -c Release -o ./{ArtifactsDir}", echoPrefix: Prefix);
                    
                    CopyArtifacts();
                });


                Target("default", DependsOn(Test, Pack));
                RunTargetsAndExit(app.RemainingArguments, logPrefix: Prefix);
            });

            app.Execute(args);
        }

        private static void Sign(string extension, string directory)
        {
            var signClientConfig = Environment.GetEnvironmentVariable("SignClientConfig");
            var signClientSecret = Environment.GetEnvironmentVariable("SignClientSecret");

            if (string.IsNullOrWhiteSpace(signClientConfig))
            {
                throw new Exception("SignClientConfig environment variable is missing. Aborting.");
            }

            if (string.IsNullOrWhiteSpace(signClientSecret))
            {
                throw new Exception("SignClientSecret environment variable is missing. Aborting.");
            }

            var files = Directory.GetFiles(directory, extension, SearchOption.AllDirectories);
            if (files.Count() == 0)
            {
                throw new Exception($"File to sign not found: {extension}");
            }

            foreach (var file in files)
            {
                Console.WriteLine("  Signing " + file);
                Run("dotnet", $"SignClient sign -c {signClientConfig} -i {file} -r sc-ids@dotnetfoundation.org -s \"{signClientSecret}\" -n 'IdentityServer4'", noEcho: true);
            }
        }

        private static void CopyArtifacts()
        {
            var files = Directory.GetFiles($"./{ArtifactsDir}");

            foreach (string s in files)
            {
                var fileName = Path.GetFileName(s);
                var destFile = Path.Combine("../../nuget", fileName);
                File.Copy(s, destFile, true);
            }
        }

        private static void CleanArtifacts()
        {
            Directory.CreateDirectory($"./{ArtifactsDir}");

            foreach (var file in Directory.GetFiles($"./{ArtifactsDir}"))
            {
                File.Delete(file);
            }
        }
    }
}