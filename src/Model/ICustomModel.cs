using System;

namespace SideLoader
{
    public interface ICustomModel
    {
        Type SLTemplateModel { get; }
        Type GameModel { get; }
    }
}
