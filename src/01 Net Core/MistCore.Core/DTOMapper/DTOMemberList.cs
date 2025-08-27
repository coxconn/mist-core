using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Core.DTOMapper
{
    //
    // 摘要:
    //     Member list to check for configuration validation
    public enum DTOMemberList
    {
        //
        // 摘要:
        //     Check that all destination members are mapped
        Destination = 0,
        //
        // 摘要:
        //     Check that all source members are mapped
        Source = 1,
        //
        // 摘要:
        //     Check neither source nor destination members, skipping validation
        None = 2
    }
}
