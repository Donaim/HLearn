using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public abstract class Secret : SingleSpell
    {
        public Secret(Player parent) : base(parent)
        {

        }

        protected override bool CustomCanPlay(out Action onplay)
        {
            onplay = AddSecred;
            return true;
        }

        protected abstract void AddSecred();
    }
}
