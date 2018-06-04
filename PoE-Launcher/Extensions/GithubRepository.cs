using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoELauncher.Extensions
{

    public class GithubRepository
    {
        public string Owner { get; set; }
        public string App { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CurrentVersion { get; set; }
        public string Exe { get; set; }
        public string latestVersionLink { get; set; }

        public event EventClick ButtonClick;
        public void OnClick()
        {
            EventClick handler = ButtonClick;
            VarEventClick e = new VarEventClick();
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
