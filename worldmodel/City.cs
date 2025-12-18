using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace worldmodel;

[Table("city")]
public partial class City
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("CountryId")]
    public int CountryId { get; set; }

    [Column("name")]
    [StringLength(500)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Column("latitutde")]
    public int Latitude { get; set; }

    [Column("longtitude")]
    public int Longitude { get; set; }

    [Column("population")]
    public int Population { get; set; }

    [ForeignKey("CountryId")]
    [InverseProperty("Cities")]
    public virtual Country Country { get; set; } = null!;
}
