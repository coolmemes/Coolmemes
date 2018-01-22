using Butterfly.Core;
using ButterStorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Butterfly.HabboHotel.Users.Clothing
{
    class UserLook
    {
        private Dictionary<int, List<Palette>> PaletteColors;
        private Dictionary<string, List<Clothes>> Clothes;

        private XmlTextReader reader;
        private XmlNodeType type;

        public void Load()
        {
            this.PaletteColors = new Dictionary<int, List<Palette>>();
            this.Clothes = new Dictionary<string, List<Clothing.Clothes>>();

            reader = new XmlTextReader("Settings/" + EmuSettings.FIGUREDATA_LINK);

            ReadMore();

            // <xml>
            if (type == XmlNodeType.XmlDeclaration)
            {
                ReadMore();

                // <figuredata>
                if (type == XmlNodeType.Element && reader.Name == "figuredata")
                {
                    while (ReadMore())
                    {
                        // <colors>
                        if (type == XmlNodeType.Element && reader.Name == "colors")
                        {
                            while (ReadMore())
                            {
                                if (type == XmlNodeType.EndElement && reader.Name == "colors")
                                    break;

                                // <palette>
                                if (type == XmlNodeType.Element && reader.Name == "palette")
                                {
                                    int PaletteId = int.Parse(reader.GetAttribute("id"));

                                    // Añadimos este Id a la paleta de colores.
                                    this.PaletteColors.Add(PaletteId, new List<Palette>());

                                    while (ReadMore())
                                    {
                                        // <color>
                                        if (type == XmlNodeType.Element && reader.Name == "color")
                                        {
                                            // Leémos los atributos
                                            int ColorID = int.Parse(reader.GetAttribute("id"));
                                            int IndexID = int.Parse(reader.GetAttribute("index"));
                                            int Club = int.Parse(reader.GetAttribute("club"));
                                            bool Selectable = OtanixEnvironment.EnumToBool(reader.GetAttribute("selectable"));

                                            // Añadimos el color a la paleta.
                                            // Console.WriteLine("Log: ColorId [" + ColorID + "], IndexID [" + IndexID + "], Club [" + Club + "], Selectable [" + Selectable + "]");
                                            this.PaletteColors[PaletteId].Add(new Palette(PaletteId, ColorID, IndexID, Club, Selectable));
                                        }
                                        else if (type == XmlNodeType.EndElement && reader.Name == "palette")
                                        {
                                            // Terminamos con el <palette>
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (type == XmlNodeType.Element && reader.Name == "sets")
                        {
                            while (ReadMore())
                            {
                                if (type == XmlNodeType.EndElement && reader.Name == "sets")
                                    break;

                                // <settype>
                                if (type == XmlNodeType.Element && reader.Name == "settype")
                                {
                                    string Type = reader.GetAttribute("type");
                                    int PaletteId = int.Parse(reader.GetAttribute("paletteid"));

                                    this.Clothes.Add(Type, new List<Clothing.Clothes>());

                                    while (ReadMore())
                                    {
                                        // <set>
                                        if (type == XmlNodeType.Element && reader.Name == "set")
                                        {
                                            // Leémos los atributos
                                            int Id = int.Parse(reader.GetAttribute("id"));
                                            string Gender = reader.GetAttribute("gender");
                                            int Club = int.Parse(reader.GetAttribute("club"));
                                            bool Colorable = OtanixEnvironment.EnumToBool(reader.GetAttribute("colorable"));
                                            bool Sellable = OtanixEnvironment.EnumToBool(reader.GetAttribute("sellable"));
                                            // ...

                                            // Ropas
                                            // Console.WriteLine("Log: Id [" + Id + "], Gender [" + Gender + "], Club [" + Club + "], Colorable [" + Colorable + "]");
                                            this.Clothes[Type].Add(new Clothing.Clothes(Type, PaletteId, Id, Gender, Club, Colorable, Sellable));

                                            // <part>
                                        }
                                        else if (type == XmlNodeType.EndElement && reader.Name == "settype")
                                        {
                                            // Terminamos con el <settype>
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool ReadMore()
        {
            bool CanRead = false;

            CanRead = reader.Read();
            type = reader.NodeType;

            if (type == XmlNodeType.Whitespace)
                return ReadMore();

            return CanRead;
        }

        public bool IsValidLook(Habbo Habbo, string Look)
        {
            if (ValidateLook(Habbo, Look))
                return true;

            Logging.LogReport("invalidlook.txt", Habbo.Username + ": " + Look);
            return false;
        }

        private bool ValidateLook(Habbo Habbo, string Look)
        {
            // Si no hay look.
            if (string.IsNullOrEmpty(Look))
                return false;

            // Partes imprescindibles.
            if (!Look.Contains("hd") || !Look.Contains("ch") || !Look.Contains("lg"))
                return false;

            // Partes
            string[] Sets = Look.Split('.');

            foreach (string Set in Sets)
            {
                string[] Parts = Set.Split('-');

                // Número de partes inválido.
                if (Parts.Length < 2 || Parts.Length > 4)
                    return false;

                string ClothesName = Parts[0];
                int ClothesId = int.Parse(Parts[1]);
                int ClothesColor = -1;

                // Si esta ropa no existe.
                if (!Clothes.ContainsKey(ClothesName))
                    return false;

                Clothes Ropa = Clothes[ClothesName].Where(t => t.Id == ClothesId).First();
                if (Ropa == null)
                    return false;

                // Si no es de varios sexos.
                if (Ropa.Gender.ToUpper() != "U")
                {
                    // Género Inválido.
                    if (Ropa.Gender.ToUpper() != Habbo.Gender.ToUpper())
                        return false;
                }

                if (Ropa.Colorable)
                {
                    // Si no hay 3 partes en la ropa.
                    if (Parts.Length != 3 && Parts.Length != 4)
                        return false;

                    // Comprobamos los colores.
                    for (int i = 2; i < Parts.Length; i++)
                    {
                        ClothesColor = int.Parse(Parts[i]);

                        // Si la paletta de colores no existe.
                        if (!PaletteColors.ContainsKey(Ropa.PaletteId))
                            return false;

                        // Si este color no está en la paleta.
                        if (!PaletteColors[Ropa.PaletteId].Exists(t => t.Id == ClothesColor))
                            return false;
                    }
                }
                else
                {
                    // Si hay color cuando esta ropa no es coloreable.
                    if (Parts.Length != 2)
                        return false;
                }

                if (Ropa.Sellable)
                {
                    // La ropa es premium y no está comprada.
                    if (!Habbo.GetUserClothingManager().ContainsPart(ClothesId))
                        return false;
                }
            }

            return true;
        }
    }
}