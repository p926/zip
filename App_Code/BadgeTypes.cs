using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using Instadose;
using Instadose.Data;

namespace Portal.Instadose
{
    public static class BadgeTypes
    {
        public static List<BadgeType> GetReadAnalysisBadgeTypes()
        {
            return new List<BadgeType>
            {
                new BadgeType() { Id = 1, Code = "ID1", Name = "Instadose 1" },
                new BadgeType() { Id = 9, Code = "ID1", Name = "Instadose 1" },
                new BadgeType() { Id = 11, Code = "ID2", Name = "Instadose 2" },
                new BadgeType() { Id = 10, Code = "ID+", Name = "Instadose Plus" },
                new BadgeType() { Id = 18, Code = "IDVue", Name = "Instadose VUE" }
            };
        }
    }

    public class BadgeType
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

}


