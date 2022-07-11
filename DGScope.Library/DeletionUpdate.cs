using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class DeletionUpdate : Update
    {
        public override UpdateType UpdateType => UpdateType.Deletion;

        public DeletionUpdate() { }
        public DeletionUpdate(IUpdatable baseObject)
        {
            Base = baseObject;
        }
        public DeletionUpdate(Guid guid)
        {
            Guid = guid;
        }
    }
}
