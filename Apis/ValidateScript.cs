using BeforeOurTime.Repository.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface into the game
    /// </summary>
    public partial class Api
    {
        /// <summary>
        /// Validate script implements correct handlers and is valid javascript
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool ValidateScript(Item item)
        {
            var valid = false;

            return valid;
        }
    }
}
