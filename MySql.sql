            string param = "Hello from Hangfire!";
            BackgroundJob.Enqueue(() => MethodWithParameters(param));
