using Eco.Core.Plugins.Interfaces;

namespace EcoWorldEdit
{
    public class EcoWorldEdit : IModKitPlugin, IServerPlugin
    {
        public string GetStatus()
        {
            return "";
        }

        public override string ToString()
        {
            return "EcoWorldEdit";
        }
    }
}
