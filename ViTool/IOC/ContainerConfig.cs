﻿using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViTool.IOC.Modules;

namespace ViTool.IOC
{
    public class ContainerConfig
    {
        public static IContainer Configure()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(ViewModelsModule).Assembly);

            return builder.Build();
        }
    }
}
