using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    public interface ICustomModel
    {
        Type SLTemplateModel { get; }
        Type GameModel { get; }
    }
}
