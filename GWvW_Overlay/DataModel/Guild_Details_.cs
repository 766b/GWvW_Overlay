namespace GWvW_Overlay.DataModel
{
    /* Sample https://api.guildwars2.com/v1/guild_details.json?guild_id=F4CFE0DD-69BF-47DE-8CAA-12E712EE6430
     * {
     *  "guild_id":"F4CFE0DD-69BF-47DE-8CAA-12E712EE6430",
     *  "guild_name":"Pain Train Choo",
     *  "tag":"Choo",
     *  "emblem":{"background_id":5,"foreground_id":107,"flags":[],"background_color_id":473,"foreground_primary_color_id":64,"foreground_secondary_color_id":473}
     *  }
     */

    public class Guild_Details_
    {
        public string guild_id { get; set; }
        public string guild_name { get; set; }
        public string tag { get; set; }
        //public Emblem emblem { get; set; } /* Skip */
    }
}
