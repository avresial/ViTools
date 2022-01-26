using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViTool.Models;

namespace ViTool.IOC.Modules
{
    public class ModelsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<IndicatorColors>().AsSelf();
            builder.RegisterType<Settings>().AsSelf();
            
        }
    }
}
