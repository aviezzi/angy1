﻿namespace Angy.Model.Model
{
    public class Product : EntityBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MicroCategory MicroCategory { get; set; }
    }
}