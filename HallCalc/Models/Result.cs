using System.Collections.Generic;
using HallCalc.Models.Results;

namespace HallCalc.Models;

public class Result
{
    public int Rank {get; set;}
    public int OppIvs { get; set; }
    public int OppLevel { get; set; }
    public string? Probability { get; set; }
    public List<CalcResult> Attacking {get;set;} = [];
    public List<CalcResult> Defending {get;set;} = [];
}