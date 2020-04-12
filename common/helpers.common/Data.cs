using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;
using helpers.extensions;
using System.Net;
using System.Web.Script.Serialization;

namespace helpers.data
{
    public class Data
    {
        class Logger : helpers.Logger
        {
            public Logger()
                : base("data requests")
            { }
        }
        abstract private class Request
        {
            private class Template
            {
                public DateTime dt = DateTime.MinValue;
                public DateTime dtNext
                {
                    get
                    {
                        return dt.Add(tsInterval);
                    }
                }
                public TimeSpan tsInterval = TimeSpan.FromMinutes(1);
                public int nBuild = 0;
                public XmlNode cValue;
            }
            public class News : Request
            {
				private Template _cTemplate = new Template() { tsInterval = TimeSpan.FromMinutes(5) };

                public override XmlNode this[byte nTemplateID, object oValue]
                {
                    get
                    {
                        if (0 != nTemplateID)
                            throw new Exception("unknown news request template [" + nTemplateID + "]");
                        return Yandex();
                    }
                }
                private XmlNode Yandex()
                {
                    if (DateTime.Now >= _cTemplate.dtNext)
                    {
                        int nBuild;
                        XmlNode cResult;
                        XmlNode[] aItems;
                        XmlDocument cXmlDocument = new XmlDocument();
                        cXmlDocument.LoadXml((new System.Net.WebClient() { Encoding = Encoding.UTF8 }).DownloadString("http://news.yandex.ru/index.rss"));
                        nBuild = cXmlDocument.NodeGet("rss/channel/lastBuildDate").InnerText.GetHashCode();
                        if (_cTemplate.nBuild != nBuild)
                        {
                            aItems = cXmlDocument.NodesGet("rss/channel/item", false);
                            if (null != aItems)
                            {
                                _cTemplate.nBuild = nBuild;
                                cXmlDocument = new XmlDocument();
                                cResult = cXmlDocument.CreateNode(XmlNodeType.Element, "result", null);
                                XmlNode cXNItem;
                                foreach (string sItem in aItems.Select(o => o.NodeGet("title").InnerText).ToArray())
                                {
                                    cXNItem = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
                                    cXNItem.InnerText = sItem.StripTags() + ".    ";
                                    cResult.AppendChild(cXNItem);
                                }
                                _cTemplate.cValue = cResult;
                                _cTemplate.dt = DateTime.Now;
                            }
                            else
                                (new Logger()).WriteWarning("can't get any news from rss");
                        }
                    }
                    return _cTemplate.cValue;
                }
            }
            public class Weather : Request
            {
				public class YandexResult
				{
					public class IdNamePair
					{
						public long id;
						public string name;
					}
					public class Geo
					{
						public IdNamePair locality;
						public IdNamePair country;
					}
					public class Info
					{
						public long geoid;
					}
					public class Forecast
					{
						public class Item
						{
							public DateTime dtObs_time
							{
								get
								{
									if (obs_time > 0)
										return (new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).AddSeconds(obs_time).ToLocalTime();  // obs_time/1000  for java timestamp
									else
										return DateTime.MinValue;
								}
							}
							public float wind_speed;
							public string wind_dir;
							public int pressure_mm;
							public int humidity;
							public string condition;
							public long obs_time;
							public int temp;
							public int temp_avg;
							public int temp_min;
							public int temp_max;
							public string hour;
							public long hour_ts;
						}
						public class Parts
						{
							public Item DayPartGet(Request.Weather.Period ePeriod)
							{
								switch (ePeriod)
								{
									case Period.Morning:
										return morning;
									case Period.Day:
										return day;
									case Period.Evening:
										return evening;
									case Period.Night:
										return night;
									default:
										return day;
								}
							}
							public Item night;
							public Item morning;
							public Item day;
							public Item evening;
							public Item day_short;
							public Item night_short;
						}
						public DateTime dtDate
						{
							get
							{
								if (date_ts > 0)
									return (new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).AddSeconds(date_ts).ToLocalTime();  // obs_time/1000  for java timestamp
								else
									return DateTime.MinValue;
							}
						}
						public long date_ts;
						public string sunrise;
						public string sunset;
						public string moonrise;
						public string moonset;
						public string moon_text;
						public Parts parts;
						public Item[] hours;
					}
					public Info info;
					public Geo geo_object;
					public Forecast.Item fact;
					public Forecast[] forecasts;
					public static YandexResult ForecastGet(string nGeoID, string sWeatherKey)
					{
						string url = "https://api.weather.yandex.ru/v1/forecast?geoid=" + nGeoID;
						WebClient cWebClient;
						cWebClient = new WebClient();
						cWebClient.Headers.Add("Accept", "*/*");
						cWebClient.Headers.Add("User-Agent", "runscope/0.1");
						cWebClient.Headers.Add("Accept-Encoding", "deflate");
						cWebClient.Headers.Add("content-type", "text/html; charset=UTF-8");
						cWebClient.Headers.Add("X-Yandex-Weather-Key", sWeatherKey); //  key like this "12345aaa-123a-12a3-4567-aa8aa0123a45"
						string text = cWebClient.DownloadString(url);

						JavaScriptSerializer json_serializer = new JavaScriptSerializer();
						YandexResult cRetVal = json_serializer.Deserialize<YandexResult>(text);
						cRetVal.fact.temp_avg = int.MinValue;
						return cRetVal;
					}
				}
				private class Region : Template
                {
                    static public Dictionary<string, string> ahYandexIDs;
					static public Dictionary<string, string> ahYandexTranslations;
					static private Dictionary<string, string> ahYandexGrouping;
                    static private Dictionary<Type, string> ahYandexIcons;
                    private Dictionary<Period, Data> _ahPeriods;

                    public string sID;
                    public string sName;
                    public Data this[Period ePeriod]
                    {
                        get
                        {
                            if (_ahPeriods.ContainsKey(ePeriod))
                                return _ahPeriods[ePeriod];
                            return null;
                        }
                        set
                        {
                            if (_ahPeriods.ContainsKey(ePeriod))
                                _ahPeriods[ePeriod] = value;
                            else
                                _ahPeriods.Add(ePeriod, value);
                        }
                    }
					static public void RegionInit()
					{
						if (null == Region.ahYandexIDs)
						{
							Region.ahYandexIDs = new Dictionary<string, string>();
							#region cities_dictionary_initiating
							Region.ahYandexIDs.Add("абакан", "1095");
							Region.ahYandexIDs.Add("абхазия", "29386");
							Region.ahYandexIDs.Add("австралия и океания", "138");
							Region.ahYandexIDs.Add("австралия", "211");
							Region.ahYandexIDs.Add("австрия", "113");
							Region.ahYandexIDs.Add("азербайджан", "167");
							Region.ahYandexIDs.Add("азия", "183");
							Region.ahYandexIDs.Add("акмолинская область", "29403");
							Region.ahYandexIDs.Add("актобе", "20273");
							Region.ahYandexIDs.Add("актюбинская область", "29404");
							Region.ahYandexIDs.Add("александров", "10656");
							Region.ahYandexIDs.Add("алматинская область", "29406");
							Region.ahYandexIDs.Add("алматы", "162");
							Region.ahYandexIDs.Add("алтайский край", "11235");
							Region.ahYandexIDs.Add("алушта", "11471");
							Region.ahYandexIDs.Add("альметьевск", "11121");
							Region.ahYandexIDs.Add("амурская область", "11375");
							Region.ahYandexIDs.Add("анадырь", "11458");
							Region.ahYandexIDs.Add("анапа", "1107");
							Region.ahYandexIDs.Add("ангарск", "11256");
							Region.ahYandexIDs.Add("апатиты", "10894");
							Region.ahYandexIDs.Add("аргентина", "93");
							Region.ahYandexIDs.Add("арзамас", "11080");
							Region.ahYandexIDs.Add("арктика и антарктика", "245");
							Region.ahYandexIDs.Add("армавир", "10987");
							Region.ahYandexIDs.Add("армения", "168");
							Region.ahYandexIDs.Add("архангельск", "20");
							Region.ahYandexIDs.Add("архангельская область", "10842");
							Region.ahYandexIDs.Add("астана", "163");
							Region.ahYandexIDs.Add("астраханская область", "10946");
							Region.ahYandexIDs.Add("астрахань", "37");
							Region.ahYandexIDs.Add("атланта", "86");
							Region.ahYandexIDs.Add("атырауская область", "29407");
							Region.ahYandexIDs.Add("африка", "241");
							Region.ahYandexIDs.Add("ачинск", "11302");
							Region.ahYandexIDs.Add("балаково", "11143");
							Region.ahYandexIDs.Add("балашиха", "10716");
							Region.ahYandexIDs.Add("барнаул", "197");
							Region.ahYandexIDs.Add("беер-шева", "129");
							Region.ahYandexIDs.Add("беларусь", "149");
							Region.ahYandexIDs.Add("белая церковь", "10369");
							Region.ahYandexIDs.Add("белгород", "4");
							Region.ahYandexIDs.Add("белгородская область", "10645");
							Region.ahYandexIDs.Add("белогорск", "11374");
							Region.ahYandexIDs.Add("бельгия", "114");
							Region.ahYandexIDs.Add("бельцы", "10314");
							Region.ahYandexIDs.Add("бендеры", "10315");
							Region.ahYandexIDs.Add("бердск", "11314");
							Region.ahYandexIDs.Add("берлин", "177");
							Region.ahYandexIDs.Add("бийск", "975");
							Region.ahYandexIDs.Add("биробиджан", "11393");
							Region.ahYandexIDs.Add("благовещенск", "77");
							Region.ahYandexIDs.Add("ближний восток", "1004");
							Region.ahYandexIDs.Add("болгария", "115");
							Region.ahYandexIDs.Add("бостон", "223");
							Region.ahYandexIDs.Add("бразилия", "94");
							Region.ahYandexIDs.Add("братск", "976");
							Region.ahYandexIDs.Add("брест", "153");
							Region.ahYandexIDs.Add("брестская область", "29632");
							Region.ahYandexIDs.Add("брянск", "191");
							Region.ahYandexIDs.Add("брянская область", "10650");
							Region.ahYandexIDs.Add("бугульма", "11122");
							Region.ahYandexIDs.Add("вашингтон", "87");
							Region.ahYandexIDs.Add("великие луки", "10928");
							Region.ahYandexIDs.Add("великий новгород", "24");
							Region.ahYandexIDs.Add("великобритания", "102");
							Region.ahYandexIDs.Add("венгрия", "116");
							Region.ahYandexIDs.Add("видное", "10719");
							Region.ahYandexIDs.Add("винница", "963");
							Region.ahYandexIDs.Add("винницкая область", "20545");
							Region.ahYandexIDs.Add("витебск", "154");
							Region.ahYandexIDs.Add("витебская область", "29633");
							Region.ahYandexIDs.Add("владивосток", "75");
							Region.ahYandexIDs.Add("владикавказ", "33");
							Region.ahYandexIDs.Add("владимир", "192");
							Region.ahYandexIDs.Add("владимирcкая область", "10658");
							Region.ahYandexIDs.Add("волгоград", "38");
							Region.ahYandexIDs.Add("волгоградская область", "10950");
							Region.ahYandexIDs.Add("волгодонск", "11036");
							Region.ahYandexIDs.Add("волжский", "10951");
							Region.ahYandexIDs.Add("вологда", "21");
							Region.ahYandexIDs.Add("вологодская область", "10853");
							Region.ahYandexIDs.Add("волынская область", "20550");
							Region.ahYandexIDs.Add("воронеж", "193");
							Region.ahYandexIDs.Add("воронежcкая область", "10672");
							Region.ahYandexIDs.Add("восточно-казахстанская область", "29408");
							Region.ahYandexIDs.Add("выборг", "969");
							Region.ahYandexIDs.Add("выкса", "20040");
							Region.ahYandexIDs.Add("гамбург", "178");
							Region.ahYandexIDs.Add("гатчина", "10867");
							Region.ahYandexIDs.Add("гейдельберг", "97");
							Region.ahYandexIDs.Add("геленджик", "10990");
							Region.ahYandexIDs.Add("германия", "96");
							Region.ahYandexIDs.Add("глазов", "11150");
							Region.ahYandexIDs.Add("гомель", "155");
							Region.ahYandexIDs.Add("гомельская область", "29631");
							Region.ahYandexIDs.Add("горно-алтайск", "11319");
							Region.ahYandexIDs.Add("греция", "246");
							Region.ahYandexIDs.Add("гродненская область", "29634");
							Region.ahYandexIDs.Add("гродно", "10274");
							Region.ahYandexIDs.Add("грозный", "1106");
							Region.ahYandexIDs.Add("грузия", "169");
							Region.ahYandexIDs.Add("гусь-хрустальный", "10661");
							Region.ahYandexIDs.Add("дальний восток", "73");
							Region.ahYandexIDs.Add("дания", "203");
							Region.ahYandexIDs.Add("детройт", "89");
							Region.ahYandexIDs.Add("дзержинск", "972");
							Region.ahYandexIDs.Add("димитровград", "11155");
							Region.ahYandexIDs.Add("дмитров", "10723");
							Region.ahYandexIDs.Add("днепропетровск", "141");
							Region.ahYandexIDs.Add("днепропетровская область", "20537");
							Region.ahYandexIDs.Add("долгопрудный", "214");
							Region.ahYandexIDs.Add("домодедово", "10725");
							Region.ahYandexIDs.Add("донецк", "142");
							Region.ahYandexIDs.Add("донецкая область", "20536");
							Region.ahYandexIDs.Add("дубна", "215");
							Region.ahYandexIDs.Add("евпатория", "11463");
							Region.ahYandexIDs.Add("еврейская автономная область", "10243");
							Region.ahYandexIDs.Add("европа", "111");
							Region.ahYandexIDs.Add("египет", "1056");
							Region.ahYandexIDs.Add("ейск", "10993");
							Region.ahYandexIDs.Add("екатеринбург", "54");
							Region.ahYandexIDs.Add("елабуга", "11123");
							Region.ahYandexIDs.Add("ессентуки", "11057");
							Region.ahYandexIDs.Add("жамбылская область", "29409");
							Region.ahYandexIDs.Add("железногорск", "20086");
							Region.ahYandexIDs.Add("железнодорожный", "21622");
							Region.ahYandexIDs.Add("жигулевск", "11132");
							Region.ahYandexIDs.Add("житомир", "10343");
							Region.ahYandexIDs.Add("житомирская область", "20547");
							Region.ahYandexIDs.Add("жодино", "26034");
							Region.ahYandexIDs.Add("жуковский", "20571");
							Region.ahYandexIDs.Add("забайкальский край", "21949");
							Region.ahYandexIDs.Add("закарпатская область", "20530");
							Region.ahYandexIDs.Add("западно-казахстанская область", "29410");
							Region.ahYandexIDs.Add("запорожская область", "20539");
							Region.ahYandexIDs.Add("запорожье", "960");
							Region.ahYandexIDs.Add("зеленоград", "216");
							Region.ahYandexIDs.Add("зеленодольск", "11125");
							Region.ahYandexIDs.Add("златоуст", "11202");
							Region.ahYandexIDs.Add("ивано-франковск", "10345");
							Region.ahYandexIDs.Add("ивано-франковская область", "20532");
							Region.ahYandexIDs.Add("иваново", "5");
							Region.ahYandexIDs.Add("ивановская область", "10687");
							Region.ahYandexIDs.Add("иерусалим", "130");
							Region.ahYandexIDs.Add("ижевск", "44");
							Region.ahYandexIDs.Add("израиль", "181");
							Region.ahYandexIDs.Add("индия", "994");
							Region.ahYandexIDs.Add("иркутск", "63");
							Region.ahYandexIDs.Add("иркутская область", "11266");
							Region.ahYandexIDs.Add("испания", "204");
							Region.ahYandexIDs.Add("италия", "205");
							Region.ahYandexIDs.Add("ишим", "11173");
							Region.ahYandexIDs.Add("йошкар-ола", "41");
							Region.ahYandexIDs.Add("казань", "43");
							Region.ahYandexIDs.Add("казахстан", "159");
							Region.ahYandexIDs.Add("кайеркан", "11306");
							Region.ahYandexIDs.Add("калининград", "22");
							Region.ahYandexIDs.Add("калининградская область", "10857");
							Region.ahYandexIDs.Add("калуга", "6");
							Region.ahYandexIDs.Add("калужская область", "10693");
							Region.ahYandexIDs.Add("каменск-уральский", "11164");
							Region.ahYandexIDs.Add("каменск-шахтинский", "11043");
							Region.ahYandexIDs.Add("камчатский край", "11398");
							Region.ahYandexIDs.Add("канада", "95");
							Region.ahYandexIDs.Add("караганда", "164");
							Region.ahYandexIDs.Add("карагандинская область", "29411");
							Region.ahYandexIDs.Add("карачаево-черкесская республика", "11020");
							Region.ahYandexIDs.Add("кельн", "98");
							Region.ahYandexIDs.Add("кемерово", "64");
							Region.ahYandexIDs.Add("кемеровская область", "11282");
							Region.ahYandexIDs.Add("керчь", "11464");
							Region.ahYandexIDs.Add("киев", "143");
							Region.ahYandexIDs.Add("киевская область", "20544");
							Region.ahYandexIDs.Add("кипр", "20574");
							Region.ahYandexIDs.Add("киргизия", "207");
							Region.ahYandexIDs.Add("киров", "46");
							Region.ahYandexIDs.Add("кирово-чепецк", "11071");
							Region.ahYandexIDs.Add("кировоград", "20221");
							Region.ahYandexIDs.Add("кировоградская область", "20548");
							Region.ahYandexIDs.Add("кировская область", "11070");
							Region.ahYandexIDs.Add("кисловодск", "11062");
							Region.ahYandexIDs.Add("китай", "134");
							Region.ahYandexIDs.Add("кишинев", "10313");
							Region.ahYandexIDs.Add("клин", "10733");
							Region.ahYandexIDs.Add("ковров", "10664");
							Region.ahYandexIDs.Add("кокшетау", "20809");
							Region.ahYandexIDs.Add("коломна", "10734");
							Region.ahYandexIDs.Add("комрат", "33883");
							Region.ahYandexIDs.Add("комсомольск-на-амуре", "11453");
							Region.ahYandexIDs.Add("корея", "135");
							Region.ahYandexIDs.Add("королёв", "20728");
							Region.ahYandexIDs.Add("костанайская область", "29412");
							Region.ahYandexIDs.Add("кострома", "7");
							Region.ahYandexIDs.Add("костромская область", "10699");
							Region.ahYandexIDs.Add("краматорск", "20554");
							Region.ahYandexIDs.Add("красногорск", "10735");
							Region.ahYandexIDs.Add("краснодар", "35");
							Region.ahYandexIDs.Add("краснодарский край", "10995");
							Region.ahYandexIDs.Add("красноярск", "62");
							Region.ahYandexIDs.Add("красноярский край", "11309");
							Region.ahYandexIDs.Add("кременчуг", "21609");
							Region.ahYandexIDs.Add("кривой рог", "10347");
							Region.ahYandexIDs.Add("крым", "977");
							Region.ahYandexIDs.Add("крымский федеральный округ", "115092");
							Region.ahYandexIDs.Add("кстово", "20044");
							Region.ahYandexIDs.Add("курган", "53");
							Region.ahYandexIDs.Add("курганская область", "11158");
							Region.ahYandexIDs.Add("курск", "8");
							Region.ahYandexIDs.Add("курская область", "10705");
							Region.ahYandexIDs.Add("кызыл", "11333");
							Region.ahYandexIDs.Add("кызылординская область", "29413");
							Region.ahYandexIDs.Add("латвия", "206");
							Region.ahYandexIDs.Add("липецк", "9");
							Region.ahYandexIDs.Add("липецкая область", "10712");
							Region.ahYandexIDs.Add("литва", "117");
							Region.ahYandexIDs.Add("лос-анджелес", "200");
							Region.ahYandexIDs.Add("луганск", "222");
							Region.ahYandexIDs.Add("луганская область", "20540");
							Region.ahYandexIDs.Add("луцк", "20222");
							Region.ahYandexIDs.Add("львов", "144");
							Region.ahYandexIDs.Add("львовская область", "20529");
							Region.ahYandexIDs.Add("люберцы", "10738");
							Region.ahYandexIDs.Add("магадан", "79");
							Region.ahYandexIDs.Add("магаданская область", "11403");
							Region.ahYandexIDs.Add("магнитогорск", "235");
							Region.ahYandexIDs.Add("майкоп", "1093");
							Region.ahYandexIDs.Add("макеевка", "24876");
							Region.ahYandexIDs.Add("мальта", "10069");
							Region.ahYandexIDs.Add("мангистауская область", "29414");
							Region.ahYandexIDs.Add("мариуполь", "10366");
							Region.ahYandexIDs.Add("махачкала", "28");
							Region.ahYandexIDs.Add("междуреченск", "11287");
							Region.ahYandexIDs.Add("мексика", "20271");
							Region.ahYandexIDs.Add("мелитополь", "10367");
							Region.ahYandexIDs.Add("миасс", "11212");
							Region.ahYandexIDs.Add("минеральные воды", "11063");
							Region.ahYandexIDs.Add("минск", "157");
							Region.ahYandexIDs.Add("минская область", "29630");
							Region.ahYandexIDs.Add("могилев", "158");
							Region.ahYandexIDs.Add("могилевская область", "29629");
							Region.ahYandexIDs.Add("молдова", "208");
							Region.ahYandexIDs.Add("москва и область", "1");
							Region.ahYandexIDs.Add("москва", "213");
							Region.ahYandexIDs.Add("мурманск", "23");
							Region.ahYandexIDs.Add("мурманская область", "10897");
							Region.ahYandexIDs.Add("муром", "10668");
							Region.ahYandexIDs.Add("мытищи", "10740");
							Region.ahYandexIDs.Add("мюнхен", "99");
							Region.ahYandexIDs.Add("набережные челны", "236");
							Region.ahYandexIDs.Add("назрань", "1092");
							Region.ahYandexIDs.Add("нальчик", "30");
							Region.ahYandexIDs.Add("находка", "974");
							Region.ahYandexIDs.Add("невинномысск", "11064");
							Region.ahYandexIDs.Add("ненецкий ао", "10176");
							Region.ahYandexIDs.Add("нефтекамск", "11114");
							Region.ahYandexIDs.Add("нидерланды", "118");
							Region.ahYandexIDs.Add("нижегородская область", "11079");
							Region.ahYandexIDs.Add("нижневартовск", "1091");
							Region.ahYandexIDs.Add("нижнекамск", "11127");
							Region.ahYandexIDs.Add("нижний новгород", "47");
							Region.ahYandexIDs.Add("нижний тагил", "11168");
							Region.ahYandexIDs.Add("николаев", "148");
							Region.ahYandexIDs.Add("николаевская область", "20543");
							Region.ahYandexIDs.Add("новая зеландия", "139");
							Region.ahYandexIDs.Add("новгородская область", "10904");
							Region.ahYandexIDs.Add("новокузнецк", "237");
							Region.ahYandexIDs.Add("новомосковск", "10830");
							Region.ahYandexIDs.Add("новороссийск", "970");
							Region.ahYandexIDs.Add("новосибирск", "65");
							Region.ahYandexIDs.Add("новосибирская область", "11316");
							Region.ahYandexIDs.Add("новоуральск", "11170");
							Region.ahYandexIDs.Add("новочеркасск", "238");
							Region.ahYandexIDs.Add("ногинск", "10742");
							Region.ahYandexIDs.Add("норвегия", "119");
							Region.ahYandexIDs.Add("норильск", "11311");
							Region.ahYandexIDs.Add("нью-йорк", "202");
							Region.ahYandexIDs.Add("обнинск", "967");
							Region.ahYandexIDs.Add("общероссийские", "382");
							Region.ahYandexIDs.Add("объединенные арабские эмираты", "210");
							Region.ahYandexIDs.Add("одесса", "145");
							Region.ahYandexIDs.Add("одесская область", "20541");
							Region.ahYandexIDs.Add("одинцово", "10743");
							Region.ahYandexIDs.Add("озерск", "11214");
							Region.ahYandexIDs.Add("омск", "66");
							Region.ahYandexIDs.Add("омская область", "11318");
							Region.ahYandexIDs.Add("орел", "10");
							Region.ahYandexIDs.Add("оренбург", "48");
							Region.ahYandexIDs.Add("оренбургская область", "11084");
							Region.ahYandexIDs.Add("орехово-зуево", "10745");
							Region.ahYandexIDs.Add("орловская область", "10772");
							Region.ahYandexIDs.Add("орск", "11091");
							Region.ahYandexIDs.Add("павловский посад", "10746");
							Region.ahYandexIDs.Add("павлодар", "190");
							Region.ahYandexIDs.Add("павлодарская область", "29415");
							Region.ahYandexIDs.Add("пенза", "49");
							Region.ahYandexIDs.Add("пензенская область", "11095");
							Region.ahYandexIDs.Add("первоуральск", "11171");
							Region.ahYandexIDs.Add("переславль", "10837");
							Region.ahYandexIDs.Add("пермский край", "11108");
							Region.ahYandexIDs.Add("пермь", "50");
							Region.ahYandexIDs.Add("петрозаводск", "18");
							Region.ahYandexIDs.Add("петропавловск-камчатский", "78");
							Region.ahYandexIDs.Add("поволжье", "40");
							Region.ahYandexIDs.Add("подольск", "10747");
							Region.ahYandexIDs.Add("полтава", "964");
							Region.ahYandexIDs.Add("полтавская область", "20549");
							Region.ahYandexIDs.Add("польша", "120");
							Region.ahYandexIDs.Add("приморский край", "11409");
							Region.ahYandexIDs.Add("прокопьевск", "11291");
							Region.ahYandexIDs.Add("псков", "25");
							Region.ahYandexIDs.Add("псковская область", "10926");
							Region.ahYandexIDs.Add("пушкино", "10748");
							Region.ahYandexIDs.Add("пущино", "217");
							Region.ahYandexIDs.Add("пятигорск", "11067");
							Region.ahYandexIDs.Add("раменское", "10750");
							Region.ahYandexIDs.Add("республика адыгея", "11004");
							Region.ahYandexIDs.Add("республика алтай", "10231");
							Region.ahYandexIDs.Add("республика башкортостан", "11111");
							Region.ahYandexIDs.Add("республика бурятия", "11330");
							Region.ahYandexIDs.Add("республика дагестан", "11010");
							Region.ahYandexIDs.Add("республика ингушетия", "11012");
							Region.ahYandexIDs.Add("республика кабардино-балкария", "11013");
							Region.ahYandexIDs.Add("республика калмыкия", "11015");
							Region.ahYandexIDs.Add("республика карелия", "10933");
							Region.ahYandexIDs.Add("республика коми", "10939");
							Region.ahYandexIDs.Add("республика марий эл", "11077");
							Region.ahYandexIDs.Add("республика мордовия", "11117");
							Region.ahYandexIDs.Add("республика саха (якутия)", "11443");
							Region.ahYandexIDs.Add("республика северная осетия-алания", "11021");
							Region.ahYandexIDs.Add("республика тыва", "10233");
							Region.ahYandexIDs.Add("республика хакасия", "11340");
							Region.ahYandexIDs.Add("реутов", "21621");
							Region.ahYandexIDs.Add("ржев", "10820");
							Region.ahYandexIDs.Add("ровенская область", "20534");
							Region.ahYandexIDs.Add("ровно", "10355");
							Region.ahYandexIDs.Add("россия", "225");
							Region.ahYandexIDs.Add("ростов-на-дону", "39");
							Region.ahYandexIDs.Add("ростов", "10838");
							Region.ahYandexIDs.Add("ростовская область", "11029");
							Region.ahYandexIDs.Add("рубцовск", "11251");
							Region.ahYandexIDs.Add("рыбинск", "10839");
							Region.ahYandexIDs.Add("рязанская область", "10776");
							Region.ahYandexIDs.Add("рязань", "11");
							Region.ahYandexIDs.Add("салават", "11115");
							Region.ahYandexIDs.Add("салехард", "58");
							Region.ahYandexIDs.Add("самара", "51");
							Region.ahYandexIDs.Add("самарская область", "11131");
							Region.ahYandexIDs.Add("сан-франциско", "90");
							Region.ahYandexIDs.Add("санкт-петербург и ленинградская область", "10174");
							Region.ahYandexIDs.Add("санкт-петербург", "2");
							Region.ahYandexIDs.Add("саранск", "42");
							Region.ahYandexIDs.Add("сарапул", "11152");
							Region.ahYandexIDs.Add("саратов", "194");
							Region.ahYandexIDs.Add("саратовская область", "11146");
							Region.ahYandexIDs.Add("саров", "11083");
							Region.ahYandexIDs.Add("сатис", "20258");
							Region.ahYandexIDs.Add("сатка", "11217");
							Region.ahYandexIDs.Add("сахалинская область", "11450");
							Region.ahYandexIDs.Add("саяногорск", "11341");
							Region.ahYandexIDs.Add("свердловская область", "11162");
							Region.ahYandexIDs.Add("севастополь", "959");
							Region.ahYandexIDs.Add("северная америка", "10002");
							Region.ahYandexIDs.Add("северный кавказ", "102444");
							Region.ahYandexIDs.Add("северо-запад", "17");
							Region.ahYandexIDs.Add("северо-казахстанская область", "29416");
							Region.ahYandexIDs.Add("северодвинск", "10849");
							Region.ahYandexIDs.Add("северск", "11351");
							Region.ahYandexIDs.Add("семей", "165");
							Region.ahYandexIDs.Add("сербия", "180");
							Region.ahYandexIDs.Add("сергиев посад", "10752");
							Region.ahYandexIDs.Add("серпухов", "10754");
							Region.ahYandexIDs.Add("сибирь", "59");
							Region.ahYandexIDs.Add("симферополь", "146");
							Region.ahYandexIDs.Add("сиэтл", "91");
							Region.ahYandexIDs.Add("словакия", "121");
							Region.ahYandexIDs.Add("словения", "122");
							Region.ahYandexIDs.Add("смоленск", "12");
							Region.ahYandexIDs.Add("смоленская область", "10795");
							Region.ahYandexIDs.Add("снг", "166");
							Region.ahYandexIDs.Add("снежинск", "11218");
							Region.ahYandexIDs.Add("соликамск", "11110");
							Region.ahYandexIDs.Add("солнечногорск", "10755");
							Region.ahYandexIDs.Add("сортавала", "10937");
							Region.ahYandexIDs.Add("сочи", "239");
							Region.ahYandexIDs.Add("ставрополь", "36");
							Region.ahYandexIDs.Add("ставропольский край", "11069");
							Region.ahYandexIDs.Add("старый оскол", "10649");
							Region.ahYandexIDs.Add("стерлитамак", "11116");
							Region.ahYandexIDs.Add("страны балтии", "980");
							Region.ahYandexIDs.Add("ступино", "10756");
							Region.ahYandexIDs.Add("суздаль", "10671");
							Region.ahYandexIDs.Add("сумская область", "20552");
							Region.ahYandexIDs.Add("сумы", "965");
							Region.ahYandexIDs.Add("сургут", "973");
							Region.ahYandexIDs.Add("сша", "84");
							Region.ahYandexIDs.Add("сызрань", "11139");
							Region.ahYandexIDs.Add("сыктывкар", "19");
							Region.ahYandexIDs.Add("таганрог", "971");
							Region.ahYandexIDs.Add("таджикистан", "209");
							Region.ahYandexIDs.Add("таиланд", "995");
							Region.ahYandexIDs.Add("талдыкорган", "10303");
							Region.ahYandexIDs.Add("тамбов", "13");
							Region.ahYandexIDs.Add("тамбовская область", "10802");
							Region.ahYandexIDs.Add("татарстан", "11119");
							Region.ahYandexIDs.Add("тверская область", "10819");
							Region.ahYandexIDs.Add("тверь", "14");
							Region.ahYandexIDs.Add("тель-авив", "131");
							Region.ahYandexIDs.Add("тернополь", "10357");
							Region.ahYandexIDs.Add("тернопольская область", "20531");
							Region.ahYandexIDs.Add("тирасполь", "10317");
							Region.ahYandexIDs.Add("тобольск", "11175");
							Region.ahYandexIDs.Add("тольятти", "240");
							Region.ahYandexIDs.Add("томск", "67");
							Region.ahYandexIDs.Add("томская область", "11353");
							Region.ahYandexIDs.Add("троицк", "20674");
							Region.ahYandexIDs.Add("туапсе", "1058");
							Region.ahYandexIDs.Add("тула", "15");
							Region.ahYandexIDs.Add("тульская область", "10832");
							Region.ahYandexIDs.Add("туркмения", "170");
							Region.ahYandexIDs.Add("турция", "983");
							Region.ahYandexIDs.Add("тында", "11391");
							Region.ahYandexIDs.Add("тюменская область", "11176");
							Region.ahYandexIDs.Add("тюмень", "55");
							Region.ahYandexIDs.Add("углич", "10840");
							Region.ahYandexIDs.Add("удмуртская республика", "11148");
							Region.ahYandexIDs.Add("ужгород", "10358");
							Region.ahYandexIDs.Add("узбекистан", "171");
							Region.ahYandexIDs.Add("украина", "187");
							Region.ahYandexIDs.Add("улан-удэ", "198");
							Region.ahYandexIDs.Add("ульяновск", "195");
							Region.ahYandexIDs.Add("ульяновская область", "11153");
							Region.ahYandexIDs.Add("урал", "52");
							Region.ahYandexIDs.Add("уссурийск", "11426");
							Region.ahYandexIDs.Add("усть-илимск", "11273");
							Region.ahYandexIDs.Add("усть-каменогорск", "10306");
							Region.ahYandexIDs.Add("уфа", "172");
							Region.ahYandexIDs.Add("ухта", "10945");
							Region.ahYandexIDs.Add("феодосия", "11469");
							Region.ahYandexIDs.Add("финляндия", "123");
							Region.ahYandexIDs.Add("франкфурт-на-майне", "100");
							Region.ahYandexIDs.Add("франция", "124");
							Region.ahYandexIDs.Add("хабаровск", "76");
							Region.ahYandexIDs.Add("хабаровский край", "11457");
							Region.ahYandexIDs.Add("хайфа", "132");
							Region.ahYandexIDs.Add("ханты-мансийск", "57");
							Region.ahYandexIDs.Add("ханты-мансийский ао", "11193");
							Region.ahYandexIDs.Add("харьков", "147");
							Region.ahYandexIDs.Add("харьковская область", "20538");
							Region.ahYandexIDs.Add("херсон", "962");
							Region.ahYandexIDs.Add("херсонская область", "20542");
							Region.ahYandexIDs.Add("химки", "10758");
							Region.ahYandexIDs.Add("хмельницкая область", "20535");
							Region.ahYandexIDs.Add("хмельницкий", "961");
							Region.ahYandexIDs.Add("хорватия", "10083");
							Region.ahYandexIDs.Add("центр", "3");
							Region.ahYandexIDs.Add("чебоксары", "45");
							Region.ahYandexIDs.Add("челябинск", "56");
							Region.ahYandexIDs.Add("челябинская область", "11225");
							Region.ahYandexIDs.Add("череповец", "968");
							Region.ahYandexIDs.Add("черкасская область", "20546");
							Region.ahYandexIDs.Add("черкассы", "10363");
							Region.ahYandexIDs.Add("черкесск", "1104");
							Region.ahYandexIDs.Add("чернигов", "966");
							Region.ahYandexIDs.Add("черниговская область", "20551");
							Region.ahYandexIDs.Add("черновицкая область", "20533");
							Region.ahYandexIDs.Add("черновцы", "10365");
							Region.ahYandexIDs.Add("черноголовка", "219");
							Region.ahYandexIDs.Add("черногория", "21610");
							Region.ahYandexIDs.Add("чехия", "125");
							Region.ahYandexIDs.Add("чехов", "10761");
							Region.ahYandexIDs.Add("чеченская республика", "11024");
							Region.ahYandexIDs.Add("чимкент", "221");
							Region.ahYandexIDs.Add("чистополь", "11129");
							Region.ahYandexIDs.Add("чита", "68");
							Region.ahYandexIDs.Add("чувашская республика", "11156");
							Region.ahYandexIDs.Add("чукотский автономный округ", "10251");
							Region.ahYandexIDs.Add("шахты", "11053");
							Region.ahYandexIDs.Add("швейцария", "126");
							Region.ahYandexIDs.Add("швеция", "127");
							Region.ahYandexIDs.Add("штутгарт", "101");
							Region.ahYandexIDs.Add("щелково", "10765");
							Region.ahYandexIDs.Add("электросталь", "20523");
							Region.ahYandexIDs.Add("элиста", "1094");
							Region.ahYandexIDs.Add("энгельс", "11147");
							Region.ahYandexIDs.Add("эстония", "179");
							Region.ahYandexIDs.Add("юг", "26");
							Region.ahYandexIDs.Add("южная америка", "10003");
							Region.ahYandexIDs.Add("южная осетия", "29387");
							Region.ahYandexIDs.Add("южно-казахстанская область", "29417");
							Region.ahYandexIDs.Add("южно-сахалинск", "80");
							Region.ahYandexIDs.Add("якутск", "74");
							Region.ahYandexIDs.Add("ялта", "11470");
							Region.ahYandexIDs.Add("ямало-ненецкий ао", "11232");
							Region.ahYandexIDs.Add("япония", "137");
							Region.ahYandexIDs.Add("ярославль", "16");
							Region.ahYandexIDs.Add("ярославская область", "10841");

							Region.ahYandexIDs.Add("тбилиси", "10277");
							Region.ahYandexIDs.Add("баку", "10253");
							Region.ahYandexIDs.Add("таллин", "11481");
							Region.ahYandexIDs.Add("рига", "11474");
							Region.ahYandexIDs.Add("туркменбаши", "10326");
							Region.ahYandexIDs.Add("бишкек", "10309");
							Region.ahYandexIDs.Add("ташкент", "10335");
							Region.ahYandexIDs.Add("ереван", "10262");
							Region.ahYandexIDs.Add("стамбул", "11508");
							Region.ahYandexIDs.Add("анталия", "11511");
							Region.ahYandexIDs.Add("дубаи", "11499");
							Region.ahYandexIDs.Add("пекин", "10590");
							Region.ahYandexIDs.Add("хайнань", "114559");
							Region.ahYandexIDs.Add("лимасол", "21159");
							Region.ahYandexIDs.Add("каир", "11485");
							Region.ahYandexIDs.Add("шарм аль шейх", "11487");
							Region.ahYandexIDs.Add("куршевель", "20886");
							Region.ahYandexIDs.Add("санкт-мориц", "10517");
							Region.ahYandexIDs.Add("золотые пески", "10384");
							Region.ahYandexIDs.Add("париж", "10502");
							Region.ahYandexIDs.Add("монте-карло", "21287");
							Region.ahYandexIDs.Add("ницца", "10500");
							Region.ahYandexIDs.Add("рим", "10445");
							Region.ahYandexIDs.Add("милан", "10448");
							Region.ahYandexIDs.Add("мадрид", "10435");
							Region.ahYandexIDs.Add("марбелья", "10438");
							Region.ahYandexIDs.Add("лондон", "10393");
							Region.ahYandexIDs.Add("амстердам", "10466");
							Region.ahYandexIDs.Add("хургада", "11486");
							Region.ahYandexIDs.Add("пхукет", "10622");
							Region.ahYandexIDs.Add("тенерифе", "10441");
							#endregion
						}
						if (null == Region.ahYandexTranslations)
						{
							Region.ahYandexTranslations = new Dictionary<string, string>();   // прислали из яндекса
							#region conditions_translations_init
							Region.ahYandexTranslations.Add("clear", "ясно");
							Region.ahYandexTranslations.Add("partly-cloudy", "малооблачно");
							Region.ahYandexTranslations.Add("cloudy", "облачно с прояснениями");
							Region.ahYandexTranslations.Add("overcast", "пасмурно");
							Region.ahYandexTranslations.Add("partly-cloudy-and-light-rain", "небольшой дождь");
							Region.ahYandexTranslations.Add("cloudy-and-light-rain", "небольшой дождь");
							Region.ahYandexTranslations.Add("overcast-and-light-rain", "небольшой дождь");
							Region.ahYandexTranslations.Add("partly-cloudy-and-rain", "дождь");
							Region.ahYandexTranslations.Add("cloudy-and-rain", "дождь");
							Region.ahYandexTranslations.Add("overcast-and-rain", "сильный дождь");
							Region.ahYandexTranslations.Add("overcast-thunderstorms-with-rain", "сильный дождь, гроза");
							Region.ahYandexTranslations.Add("overcast-and-wet-snow", "дождь со снегом");
							Region.ahYandexTranslations.Add("partly-cloudy-and-light-snow", "небольшой снег");
							Region.ahYandexTranslations.Add("cloudy-and-light-snow", "небольшой снег");
							Region.ahYandexTranslations.Add("overcast-and-light-snow", "небольшой снег");
							Region.ahYandexTranslations.Add("partly-cloudy-and-snow", "снег");
							Region.ahYandexTranslations.Add("cloudy-and-snow", "снег");
							Region.ahYandexTranslations.Add("overcast-and-snow", "снегопад");
							#endregion
						}
					}
                    public Region()
                    {
                        _ahPeriods = new Dictionary<Period, Data>();
                        foreach (Period cPeriod in Enum.GetValues(typeof(Period)))
                            _ahPeriods.Add(cPeriod, null);
						tsInterval = TimeSpan.FromHours(1);
                    }
                    public void Template0Item(XmlNode cXmlNode)
                    {
                        XmlNode cXN = cXmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, "item", null);
                        cXN.AttributeAdd("id", "region");
                        cXN.InnerText = sName.ToUpper();
                        cXmlNode.AppendChild(cXN);

                        cXN = cXmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, "item", null);
                        cXN.AttributeAdd("id", "temperature");
                        cXN.AttributeAdd("color", _ahPeriods[Request.Weather.Period.Now].nColor);
                        cXN.InnerText = _ahPeriods[Request.Weather.Period.Now].sTemperature;
                        cXmlNode.AppendChild(cXN);
                    }
                    public void Template1Item(XmlNode cXmlNode, string sPrefix, Period ePeriod)
                    {
                        if (null == ahYandexGrouping)
                        {
                            ahYandexGrouping = new Dictionary<string, string>();
							#region grouping_city_region
							ahYandexGrouping.Add("барнаул", "россия");
							ahYandexGrouping.Add("владивосток", "россия");
							ahYandexGrouping.Add("воронеж", "россия");
							ahYandexGrouping.Add("волгоград", "россия");
							ahYandexGrouping.Add("екатеринбург", "россия");
							ahYandexGrouping.Add("ижевск", "россия");
							ahYandexGrouping.Add("иркутск", "россия");
							ahYandexGrouping.Add("казань", "россия");
							ahYandexGrouping.Add("кемерово", "россия");
							ahYandexGrouping.Add("краснодар", "россия");
							ahYandexGrouping.Add("красноярск", "россия");
							ahYandexGrouping.Add("москва", "россия");
							ahYandexGrouping.Add("нижний новгород", "россия");
							ahYandexGrouping.Add("новосибирск", "россия");
							ahYandexGrouping.Add("омск", "россия");
							ahYandexGrouping.Add("пермь", "россия");
							ahYandexGrouping.Add("ростов-на-дону", "россия");
							ahYandexGrouping.Add("самара", "россия");
							ahYandexGrouping.Add("саратов", "россия");
							ahYandexGrouping.Add("санкт-петербург", "россия");
							ahYandexGrouping.Add("ставрополь", "россия");
							ahYandexGrouping.Add("тверь", "россия");
							ahYandexGrouping.Add("томск", "россия");
							ahYandexGrouping.Add("тула", "россия");
							ahYandexGrouping.Add("тюмень", "россия");
							ahYandexGrouping.Add("уфа", "россия");
							ahYandexGrouping.Add("хабаровск", "россия");
							ahYandexGrouping.Add("челябинск", "россия");
							ahYandexGrouping.Add("ярославль", "россия");

                            ahYandexGrouping.Add("минск", "страны  снг");
                            ahYandexGrouping.Add("тбилиси", "страны  снг");
                            ahYandexGrouping.Add("баку", "страны  снг");
                            ahYandexGrouping.Add("алматы", "страны  снг");
                            ahYandexGrouping.Add("астана", "страны  снг");
                            ahYandexGrouping.Add("таллин", "страны  снг");
                            ahYandexGrouping.Add("рига", "страны  снг");
                            ahYandexGrouping.Add("кишинев", "страны  снг");
                            ahYandexGrouping.Add("туркменбаши", "страны  снг");
                            ahYandexGrouping.Add("бишкек", "страны  снг");
                            ahYandexGrouping.Add("ташкент", "страны  снг");
                            ahYandexGrouping.Add("ереван", "страны  снг");

                            ahYandexGrouping.Add("берлин", "зарубежье");
                            ahYandexGrouping.Add("мюнхен", "зарубежье");
                            ahYandexGrouping.Add("стамбул", "зарубежье");
                            ahYandexGrouping.Add("анталия", "зарубежье");
                            ahYandexGrouping.Add("дубаи", "зарубежье");
                            ahYandexGrouping.Add("пекин", "зарубежье");
                            ahYandexGrouping.Add("хайнань", "зарубежье");
                            ahYandexGrouping.Add("лимасол", "зарубежье");
                            ahYandexGrouping.Add("каир", "зарубежье");
                            ahYandexGrouping.Add("шарм аль шейх", "зарубежье");
                            ahYandexGrouping.Add("куршевель", "зарубежье");
                            ahYandexGrouping.Add("санкт-мориц", "зарубежье");
                            ahYandexGrouping.Add("золотые пески", "зарубежье");
                            ahYandexGrouping.Add("париж", "зарубежье");
                            ahYandexGrouping.Add("монте-карло", "зарубежье");
                            ahYandexGrouping.Add("ницца", "зарубежье");
                            ahYandexGrouping.Add("рим", "зарубежье");
                            ahYandexGrouping.Add("милан", "зарубежье");
                            ahYandexGrouping.Add("мадрид", "зарубежье");
                            ahYandexGrouping.Add("марбея", "зарубежье");
                            ahYandexGrouping.Add("лондон", "зарубежье");
                            ahYandexGrouping.Add("амстердам", "зарубежье");
							ahYandexGrouping.Add("марбелья", "зарубежье");
							ahYandexGrouping.Add("хургада", "зарубежье");
							ahYandexGrouping.Add("пхукет", "зарубежье");
							ahYandexGrouping.Add("тенерифе", "зарубежье");
                            #endregion
                        }
                        if (!ahYandexGrouping.ContainsKey(sName))
                            throw new Exception("unknown grouping for " + sName);
                        if (null == ahYandexIcons)
                        {
                            ahYandexIcons = new Dictionary<Type, string>();
							#region grouping_conditiontype_fileicon
							ahYandexIcons.Add(Type.Clouds | Type.RainHeavy, "weather_icn_bkn_+ra_d_c");
							ahYandexIcons.Add(Type.Clouds, "weather_icn_bkn_d_c");
							ahYandexIcons.Add(Type.Clouds | Type.Rain, "weather_icn_bkn_-ra_d_c");
							ahYandexIcons.Add(Type.Clouds | Type.SnowMore, "weather_icn_bkn_sn_d_c");
							ahYandexIcons.Add(Type.Clouds | Type.Snow, "weather_icn_bkn_-sn_d_c");
							ahYandexIcons.Add(Type.CloudsHeavy | Type.RainHeavy, "weather_icn_ovc_+ra_c");
							ahYandexIcons.Add(Type.CloudsHeavy | Type.SnowHeavy, "weather_icn_ovc_+sn_c");
							ahYandexIcons.Add(Type.CloudsHeavy | Type.Hail, "weather_icn_ovc_gr_c");
							ahYandexIcons.Add(Type.CloudsHeavy, "weather_icn_ovc_c");
							ahYandexIcons.Add(Type.CloudsHeavy | Type.Rain, "weather_icn_ovc_-ra_c");
							ahYandexIcons.Add(Type.CloudsHeavy | Type.SnowMore, "weather_icn_ovc_sn_c");
							ahYandexIcons.Add(Type.CloudsHeavy | Type.Snow, "weather_icn_ovc_-sn_c");
							ahYandexIcons.Add(Type.CloudsHeavy | Type.Thunderstorm | Type.Rain, "weather_icn_ovc_ts_ra_c");
							ahYandexIcons.Add(Type.CloudsHeavy | Type.Thunderstorm | Type.Rain | Type.Snow, "weather_icn_ovc_ts_ra_sn_c");
							ahYandexIcons.Add(Type.Clear, "weather_icn_skc_d_c");
							ahYandexIcons.Add(Type.Fog, "weather_icn_ovc_fg_c");
							ahYandexIcons.Add(Type.FogMore, "weather_icn_ovc_+fg_c");
							ahYandexIcons.Add(Type.Lighting, "weather_icn_lt_c");
							ahYandexIcons.Add(Type.Haze, "weather_icn_mst_c");
							#endregion
						}

                        XmlNode cXN = cXmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, "item", null);
                        cXN.AttributeAdd("id", sPrefix + "_CITY");
                        cXN.InnerText = sName.ToUpper();
                        cXmlNode.AppendChild(cXN);

                        cXN = cXmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, "item", null);
                        cXN.AttributeAdd("id", sPrefix + "_TIME");
                        switch (ePeriod)
                        {
                            case Period.Now:
                                cXN.InnerText = "СЕГОДНЯ";
                                break;
                            case Period.Tomorrow:
                                cXN.InnerText = "ЗАВТРА";
                                break;
                            default:
                                throw new NotImplementedException("unsupported period:" + ePeriod);
                        }
                        cXmlNode.AppendChild(cXN);

                        cXN = cXmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, "item", null);
                        cXN.AttributeAdd("id", sPrefix + "_TEMPERATURE");
                        cXN.InnerText = _ahPeriods[ePeriod].sTemperature;
                        cXmlNode.AppendChild(cXN);

                        cXN = cXmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, "item", null);
                        cXN.AttributeAdd("id", sPrefix + "_ICON");
						if (!ahYandexIcons.ContainsKey(_ahPeriods[ePeriod].eType))
							(new Logger()).WriteWarning("unknown yandex icon type: [" + _ahPeriods[ePeriod].eType + "]");
						cXN.InnerText = (ahYandexIcons.ContainsKey(_ahPeriods[ePeriod].eType) ? ahYandexIcons[_ahPeriods[ePeriod].eType] : ahYandexIcons[Type.Clouds]);
                        cXmlNode.AppendChild(cXN);

                        cXN = cXmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, "item", null);
                        cXN.AttributeAdd("id", sPrefix + "_DETAILS");
                        cXN.InnerText = _ahPeriods[ePeriod].sDetails;
                        cXmlNode.AppendChild(cXN);

                        cXN = cXmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, "item", null);
                        cXN.AttributeAdd("id", sPrefix + "_REGION");
                        cXN.InnerText = Region.ahYandexGrouping[sName].ToUpper();
                        cXmlNode.AppendChild(cXN);
                    }
                }
                [Flags]
                public enum Type
                {
					Clear = 0,
					Night = 1,
					Clouds = 2,
                    CloudsMore = 4,
                    CloudsHeavy = Clouds | CloudsMore,
                    Rain = 8,
                    RainMore = 16,
                    RainHeavy = Rain | RainMore,
                    Snow = 32,
                    SnowMore = 64,
                    SnowHeavy = Snow | SnowMore,
                    Thunderstorm = 128,
                    ThunderstormMore = 256,
                    ThunderstormHeavy = Thunderstorm | ThunderstormMore,
                    Hail = 512,
					Lighting = 1024,
					Fog = 2048,
					FogMore = 4096,
					Haze = 8192
                }
                public class Data
                {
					static private Type GetConditionType(string sCode)
					{
						Type eRetVal;
						#region condition
						switch (sCode)
						{
							case "cloudy-thunderstorms-with-wet-snow":
								eRetVal = Type.Clouds | Type.RainHeavy; //weather_icn_bkn_+ra_d_pal_c
								break;
							case "cloudy":
							case "thin-high-clouds":
								eRetVal = Type.Clouds; //weather_icn_bkn_d_pal_c
								break;
							case "cloudy-and-showers":
							case "cloudy-and-wet-snow-showers":
							case "cloudy-and-rain":
							case "cloudy-thunderstorms-with-rain":
							case "cloudy-and-light-rain":
							case "cloudy-and-light-wet-snow":
							case "cloudy-thunderstorms-with-snow":
								eRetVal = Type.Clouds | Type.Rain; //weather_icn_bkn_-ra_d_pal_c
								break;
							case "cloudy-and-snow":
								eRetVal = Type.Clouds | Type.SnowMore; //weather_icn_bkn_sn_d_pal_c
								break;
							case "cloudy-and-snow-showers":
							case "cloudy-and-wet-snow":
							case "cloudy-and-light-snow":
								eRetVal = Type.Clouds | Type.Snow; //weather_icn_bkn_-sn_d_pal_c
								break;
							case "intermittent-heavy-drizzle":
							case "intermittent-heavy-rain":
							case "rain-showers":
							case "violent-rain-showers":
							case "continuous-heavy-rain":
							case "moderate-to-heavy-rain-showers":
							case "light-rain-showers":
							case "funnel-clouds-within-sight":
							case "squalls-within-sight-but-no-precipitation-falling-at-station":
								eRetVal = Type.CloudsHeavy | Type.RainHeavy; //weather_icn_ovc_+ra_pal_c
								break;
							case "heavy-drifting-snow-above-eye-level":
								eRetVal = Type.CloudsHeavy | Type.SnowHeavy; //weather_icn_ovc_+sn_pal_c
								break;
							case "freezing-rain":
							case "hail-showers":
							case "heavy-thunderstorm-with-hail":
							case "moderate-to-heavy-freezing-drizzle":
							case "moderate-to-heavy-hail-showers":
							case "moderate-to-heavy-freezing-rain":
							case "light-snow-ice-pellet-showers":
							case "light-hail-showers":
							case "light-freezing-rain":
							case "light-snow-showers":
							case "snow-grains":
								eRetVal = Type.CloudsHeavy | Type.Hail; //weather_icn_ovc_gr_pal_c
								break;
							case "visibility-reduced-by-smoke":
							case "mist":
							case "clouds-dissolving":
							case "mostly-clear":
							case "state-of-sky-unchanged":
							case "mostly-clear-slight-possibility-of-rain":
							case "mostly-clear-slight-possibility-of-snow":
							case "mostly-clear-slight-possibility-of-wet-snow":
							case "overcast":
							case "clouds-developing":
							case "partly-cloudy":
							case "widespread-dust-in-suspension-not-raised-by-wind":
							case "dust-or-sand-raised-by-wind":
							case "dust-or-sand-storm-within-sight-but-not-at-station":
							case "slight-to-moderate-duststorm-decreasing-in-intensity":
							case "slight-to-moderate-duststorm-increasing-in-intensity":
							case "slight-to-moderate-duststorm-no-change":
							case "well-developed-dust-or-sand-whirls":
							case "severe-duststorm-decreasing-in-intensity":
							case "severe-duststorm-increasing-in-intensity":
							case "severe-duststorm-no-change":
								eRetVal = Type.CloudsHeavy; //weather_icn_ovc_pal_c
								break;
							case "patches-of-fog":
							case "continuous-shallow-fog":
							case "fog":
							case "fog-at-a-distance":
							case "fog-sky-visible-becoming-thicker":
							case "fog-sky-visible-no-change":
							case "fog-sky-visible-thinning":
							case "patches-of-shallow-fog":
							case "fog-depositing-rime-sky-visible":
								eRetVal = Type.Fog; //weather_icn_ovc_fg_pal_c
								break;
							case "fog-sky-not-visible-becoming-thicker":
							case "fog-sky-not-visible-no-change":
							case "fog-sky-not-visible-thinning":
							case "fog-depositing-rime-sky-not-visible":
								eRetVal = Type.FogMore; //weather_icn_ovc_+fg_pal_c
								break;
							case "intermittent-moderate-rain":
							case "intermittent-moderate-drizzle":
							case "intermittent-light-drizzle":
							case "intermittent-light-rain":
							case "continuous-moderate-rain":
							case "rain":
							case "continuous-moderate-drizzle":
							case "overcast-and-showers":
							case "overcast-and-rain":
							case "overcast-and-light-rain":
							case "distant-precipitation-but-not-falling-at-station":
							case "nearby-precipitation-but-not-falling-at-station":
							case "precipitation-within-sight-but-not-hitting-ground":
							case "partly-cloudy-and-showers":
							case "partly-cloudy-and-rain":
							case "partly-cloudy-and-light-rain":
							case "continuous-light-drizzle":
							case "light-drizzle-and-rain":
							case "continuous-light-rain":
								eRetVal = Type.CloudsHeavy | Type.Rain; //weather_icn_ovc_-ra_pal_c
								break;
							case "intermittent-heavy-snow":
							case "overcast-and-snow-showers":
							case "heavy-drifting-snow-below-eye-level":
							case "continuous-heavy-snow":
								eRetVal = Type.CloudsHeavy | Type.SnowMore; //weather_icn_ovc_sn_pal_c
								break;
							case "intermittent-light-snow":
								eRetVal = Type.CloudsHeavy | Type.Snow; //weather_icn_ovc_-sn_pal_c
								break;
							case "intermittent-moderate-snow":
							case "slight-to-moderate-drifting-snow-above-eye-level":
							case "overcast-and-light-snow":
							case "overcast-and-snow":
							case "snow-crystals":
							case "partly-cloudy-and-snow-showers":
							case "partly-cloudy-and-light-snow":
							case "partly-cloudy-and-snow":
							case "slight-to-moderate-drifting-snow-below-eye-level":
							case "cold-snap":
							case "abrupt-cold-snap":
							case "continuous-heavy-drizzle":
							case "light-freezing-drizzle":
							case "continuous-light-snow":
							case "continuous-moderate-snow":
							case "snow":
								eRetVal = Type.CloudsHeavy | Type.Snow; //weather_icn_ovc_-sn_pal_c
								break;
							case "light-to-moderate-thunderstorm":
							case "thunderstorm-but-no-precipitation-falling-at-station":
							case "thunderstorms":
							case "light-to-moderate-thunderstorm-with-hail":
							case "heavy-thunderstorm-with-duststorm":
							case "overcast-thunderstorms-with-rain":
							case "heavy-thunderstorm":
							case "thunderstorm-in-past-hour-currently-only-moderate-to-heavy-snow-or-rain-snow-mix":
							case "thunderstorm-in-past-hour-currently-only-moderate-to-heavy-rain":
							case "thunderstorm-in-past-hour-currently-only-light-rain":
								eRetVal = Type.CloudsHeavy | Type.Thunderstorm | Type.Rain; //weather_icn_ovc_ts_ra_pal_c
								break;
							case "lightning-visible-no-thunder-heard":
								eRetVal = Type.Lighting; //weather_icn_lt_pal_c
								break;
							case "rain-and-snow":
							case "ice-pellets":
							case "diamond-dust":
							case "moderate-to-heavy-snow-ice-pellet-showers":
							case "snow-showers":
							case "mostly-clear-possible-thunderstorms-with-wet-snow":
							case "mostly-clear-possible-thunderstorms-with-rain":
							case "mostly-clear-possible-thunderstorms-with-snow":
							case "drizzle":
							case "overcast-and-wet-snow-showers":
							case "overcast-and-wet-snow":
							case "overcast-thunderstorms-with-wet-snow":
							case "overcast-and-light-wet-snow":
							case "overcast-thunderstorms-with-snow":
							case "partly-cloudy-possible-thunderstorms-with-wet-snow":
							case "partly-cloudy-possible-thunderstorms-with-rain":
							case "partly-cloudy-possible-thunderstorms-with-snow":
							case "partly-cloudy-and-wet-snow-showers":
							case "partly-cloudy-and-wet-snow":
							case "partly-cloudy-and-light-wet-snow":
							case "moderate-to-heavy-drizzle-and-rain":
							case "moderate-to-heavy-rain-and-snow":
							case "moderate-to-heavy-rain-and-snow-showers":
							case "moderate-to-heavy-snow-showers":
							case "light-rain-and-snow":
							case "thunderstorm-in-past-hour-currently-only-light-snow-or-rain-snow-mix":
							case "light-rain-and-snow-showers":
								eRetVal = Type.CloudsHeavy | Type.Thunderstorm | Type.Rain | Type.Snow; //weather_icn_ovc_ts_ra_sn_pal_c
								break;
							case "haze":
								eRetVal = Type.Haze; //weather_icn_mst_pal_c
								break;
							//default:
							case "abrupt-warming":
							case "clear":
							case "clear-skies":
								eRetVal = Type.Clear; //weather_icn_skc_d_pal_c
								break;
							default:
								eRetVal = Type.Clouds;
								(new Logger()).WriteWarning("can't determine type of yandex condition: [" + sCode + "]");
								break;
						}
						#endregion condition
						return eRetVal;
					}
					static public Data Yandex(YandexResult.Forecast.Item cItem)    // с 2016 года
					{
						Data cRetVal = null;
						if (null != cItem)
						{
							cRetVal = new Data();
							cRetVal.eType = GetConditionType(cItem.condition.ToLower());
							cRetVal.sDetails = Region.ahYandexTranslations.ContainsKey(cItem.condition.ToLower()) ? Region.ahYandexTranslations[cItem.condition.ToLower()] : "";
							cRetVal.nTemperature = (cItem.temp_avg == int.MinValue ? cItem.temp : cItem.temp_avg).ToShort();
							cRetVal.nColor = uint.MaxValue;  // yandex 
						}
						return cRetVal;
					}
					static public Data Yandex(XmlNode cXmlNode)    // до конца 2015
                    {
                        Data cRetVal = null;
                        if (null != cXmlNode)
                        {
                            cRetVal = new Data();
                            string sCode = cXmlNode.NodeGet("weather_condition").AttributeValueGet("code").ToLower();
							cRetVal.eType = GetConditionType(sCode);
							cRetVal.sDetails = cXmlNode.NodeGet("weather_type").InnerText;
                            XmlNode cXN = cXmlNode.NodeGet("temperature-data", false);
                            if (null != cXN)
                            {
                                cXN = cXN.NodeGet("avg");
                                cRetVal.nColor = ("0x" + cXN.AttributeValueGet("bgcolor")).ToUInt32();
                            }
                            else
                            {
                                cXN = cXmlNode.NodeGet("temperature");
                                cRetVal.nColor = ("0x" + cXN.AttributeValueGet("color")).ToUInt32();
                            }
                            cRetVal.nTemperature = cXN.InnerText.ToShort();
                        }
                        return cRetVal;
                    }
                    public string sTemperature
                    {
                        get
                        {
							return " " + (0 < nTemperature ? "+" : "") + nTemperature + "°C";
                        }
                    }
                    public short nTemperature;
                    public uint nColor;  // в яндексе больше нет
                    public Type eType;
                    public string sDetails;
                }
                public enum Period
                {
                    Now,
                    Morning,
                    Day,
                    Evening,
                    Night,
                    Tomorrow
                }

                private List<Region> _aRegions = new List<Region>();

                public override XmlNode this[byte nTemplateID, object oValue]
                {
                    get
                    {
                        if (0 > nTemplateID || 2 < nTemplateID)
                            throw new Exception("unknown weather request template [" + nTemplateID + "]");
						string[] aRegions = ((string)oValue).Split(';');
						if (aRegions[0].StartsWith("code="))
						{
							string[] aNewRegions = new string[aRegions.Length - 1];
							Array.Copy(aRegions, 1, aNewRegions, 0, aNewRegions.Length);
                            string sCode = aRegions[0].Substring(5);
							return Yandex(nTemplateID, aNewRegions, sCode);
						}
                        return Yandex(nTemplateID, aRegions);
                    }
                }
				private XmlNode Yandex(byte nTemplateID, string[] aRegions, string sCode)   // c 1 января 2016    - теперь нужен идентификационный код от яндекса!  sCode
				{
					Region.RegionInit();
					string sRegionID;
					XmlNode cXmlNode = null;
					XmlDocument cXmlDocument = new XmlDocument();
					XmlNode cRetVal = cXmlDocument.CreateNode(XmlNodeType.Element, "result", null);
					Region cRegion = null;
					foreach (string sRegion in aRegions.Select(o => o.Trim().ToLower()))
					{
						try
						{
							if (!Region.ahYandexIDs.ContainsKey(sRegion))
								throw new Exception("unknown region [" + sRegion + "]");
							sRegionID = Region.ahYandexIDs[sRegion];
							if (null == (cRegion = _aRegions.FirstOrDefault(o => o.sID == sRegionID)) || DateTime.Now > cRegion.dtNext)
								try
								{
									YandexResult cYRes = YandexResult.ForecastGet(sRegionID, sCode);
									if (null == cRegion)
										_aRegions.Add(cRegion = new Region() { sID = sRegionID, sName = sRegion });
									cRegion.dt = DateTime.Now;
									if (null != cYRes)
									{
										if (cYRes.fact != null)
											cRegion[Request.Weather.Period.Now] = Request.Weather.Data.Yandex(cYRes.fact);
										else
											(new Logger()).WriteWarning("can't get fact-weather from yandex for " + sRegion);

										if (cYRes.forecasts != null && cYRes.forecasts.Length > 1)
										{
											foreach (Request.Weather.Period ePeriod in new Request.Weather.Period[] { Request.Weather.Period.Morning, Request.Weather.Period.Day, Request.Weather.Period.Evening, Request.Weather.Period.Night })
												cRegion[ePeriod] = Request.Weather.Data.Yandex(cYRes.forecasts[0].parts.DayPartGet(ePeriod));
											cRegion[Request.Weather.Period.Tomorrow] = Request.Weather.Data.Yandex(cYRes.forecasts[1].parts.DayPartGet(Period.Day));
										}
										else
											(new Logger()).WriteWarning("can't get forecast-weather from yandex for " + sRegion);
									}
									else
										(new Logger()).WriteWarning("can't get weather from yandex for " + sRegion);
								}
								catch (Exception ex)
								{
									(new Logger()).WriteError("region error [" + sRegion + "]", ex);
									if (null == (cRegion = _aRegions.FirstOrDefault(o => o.sID == sRegionID)))
									{
										(new Logger()).WriteWarning("region is null");
										continue;
									}
								}
							cXmlDocument = cRetVal.OwnerDocument;
							switch (nTemplateID)
							{
								case 0:
									cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
									cRegion.Template0Item(cXmlNode);
									cRetVal.AppendChild(cXmlNode);
									break;
								case 1:
									if (1 > cRetVal.ChildNodes.Count)
									{
										cRetVal.AppendChild(cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null));
										cXmlNode.AttributeAdd("output", "000");
									}
									else
										cXmlNode = cRetVal.ChildNodes[cRetVal.ChildNodes.Count - 1];

									cRegion.Template1Item(cXmlNode, "POST", Period.Now);
									cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
									cXmlNode.AttributeAdd("output", cRetVal.ChildNodes.Count.ToString("000"));
									cRegion.Template1Item(cXmlNode, "PRE", Period.Now);
									cRegion.Template1Item(cXmlNode, "POST", Period.Tomorrow);
									cRetVal.AppendChild(cXmlNode);
									cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
									cXmlNode.AttributeAdd("output", cRetVal.ChildNodes.Count.ToString("000"));
									cRegion.Template1Item(cXmlNode, "PRE", Period.Tomorrow);
									cRetVal.AppendChild(cXmlNode);
									break;
								case 2:
									//if (1 > cRetVal.ChildNodes.Count)
									//{
									//	cRetVal.AppendChild(cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null));
									//	cXmlNode.AttributeAdd("output", "000");
									//}
									//else
									//	cXmlNode = cRetVal.ChildNodes[cRetVal.ChildNodes.Count - 1];

									
									cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
									cXmlNode.AttributeAdd("output", cRetVal.ChildNodes.Count.ToString("000"));
									cRegion.Template1Item(cXmlNode, "POST", Period.Now);
									//cRegion.Template1Item(cXmlNode, "PRE", Period.Now);
									//cRegion.Template1Item(cXmlNode, "POST", Period.Tomorrow);
									cRetVal.AppendChild(cXmlNode);
									//cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
									//cXmlNode.AttributeAdd("output", cRetVal.ChildNodes.Count.ToString("000"));
									//cRegion.Template1Item(cXmlNode, "PRE", Period.Tomorrow);
									//cRetVal.AppendChild(cXmlNode);
									break;
							}
						}
						catch (Exception ex)
						{
							(new Logger()).WriteError("region error [" + sRegion + "]", ex);
						}
					}
					return cRetVal;
				}

				private XmlNode Yandex(byte nTemplateID, string[] aRegions)     // вроде как до конца 2015 года   // да! её отрубили в мае 2016!! ))
                {
					Region.RegionInit();
                    string sRegionID;
                    XmlNode cXmlNode;
                    string sValue;
                    XmlDocument cXmlDocument = new XmlDocument();
                    XmlNode cRetVal = cXmlDocument.CreateNode(XmlNodeType.Element, "result", null);
                    Region cRegion = null;
					XmlNode[] aXNDates;
					foreach (string sRegion in aRegions.Select(o => o.Trim().ToLower()))
                    {
						try
						{
							if (!Region.ahYandexIDs.ContainsKey(sRegion))
								throw new Exception("unknown region [" + sRegion + "]");
							sRegionID = Region.ahYandexIDs[sRegion];
							if (null == (cRegion = _aRegions.FirstOrDefault(o => o.sID == sRegionID)) || DateTime.Now > cRegion.dtNext)
								try
								{
									cXmlDocument = new XmlDocument();
									sValue = (new System.Net.WebClient() { Encoding = Encoding.UTF8 }).DownloadString("http://export.yandex.ru/weather-ng/forecasts-by-geo/" + sRegionID + ".xml");
									//sValue = System.IO.File.ReadAllText(@"d:\cues\blender\weather\213.xml");
									sValue = sValue.Replace("xmlns=\"http://weather.yandex.ru/forecast\"", "");
									cXmlDocument.LoadXml(sValue);
									if (null == cRegion)
										_aRegions.Add(cRegion = new Region() { sID = sRegionID, sName = sRegion });
									cRegion.dt = DateTime.Now;
									cRegion.cValue = cXmlDocument;
									cXmlNode = cXmlDocument.GetElementsByTagName("forecast")[0];
									if (null != cXmlNode)
									{
										cRegion[Request.Weather.Period.Now] = Request.Weather.Data.Yandex(cXmlNode.NodeGet("fact", false));
										aXNDates = cXmlNode.NodesGet("day[3>position()]");
										foreach (Request.Weather.Period ePeriod in new Request.Weather.Period[] { Request.Weather.Period.Morning, Request.Weather.Period.Day, Request.Weather.Period.Evening, Request.Weather.Period.Night })
											cRegion[ePeriod] = Request.Weather.Data.Yandex(aXNDates[0].NodeGet("day_part[@type='" + ePeriod.ToString().ToLower() + "']", false));
										cRegion[Request.Weather.Period.Tomorrow] = Request.Weather.Data.Yandex(aXNDates[1].NodeGet("day_part[@type='day']", false));
									}
									else
										(new Logger()).WriteWarning("can't get weather from rss for " + sRegion);
								}
								catch (Exception ex)
								{
									(new Logger()).WriteError("region error [" + sRegion + "]", ex);
									if (null == (cRegion = _aRegions.FirstOrDefault(o => o.sID == sRegionID)))
									{
										(new Logger()).WriteWarning("region is null");
										continue;
									}
								}
                            cXmlDocument = cRetVal.OwnerDocument;
                            switch (nTemplateID)
                            {
                                case 0:
                                    cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
                                    cRegion.Template0Item(cXmlNode);
                                    cRetVal.AppendChild(cXmlNode);
                                    break;
                                case 1:
                                    if (1 > cRetVal.ChildNodes.Count)
                                    {
                                        cRetVal.AppendChild(cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null));
                                        cXmlNode.AttributeAdd("output" , "000");
                                    }
                                    else
                                        cXmlNode = cRetVal.ChildNodes[cRetVal.ChildNodes.Count - 1];

                                    cRegion.Template1Item(cXmlNode, "POST", Period.Now);
                                    cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
									cXmlNode.AttributeAdd("output", cRetVal.ChildNodes.Count.ToString("000"));
                                    cRegion.Template1Item(cXmlNode, "PRE", Period.Now);
                                    cRegion.Template1Item(cXmlNode, "POST", Period.Tomorrow);
                                    cRetVal.AppendChild(cXmlNode);
                                    cXmlNode = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
									cXmlNode.AttributeAdd("output", cRetVal.ChildNodes.Count.ToString("000"));
                                    cRegion.Template1Item(cXmlNode, "PRE", Period.Tomorrow);
                                    cRetVal.AppendChild(cXmlNode);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
							(new Logger()).WriteError("region error [" + sRegion + "]", ex);
                        }
                    }
                    return cRetVal;
                }
            }
            public class Currency : Request
            {
				private Template _cTemplate = new Template() { tsInterval = TimeSpan.FromHours(6) };

                public override XmlNode this[byte nTemplateID, object oValue]
                {
                    get
                    {
                        if (0 != nTemplateID)
                            throw new Exception("unknown currency request template [" + nTemplateID + "]");
                        return CBR();
                    }
                }
                private XmlNode CBR()
                {
                    if (DateTime.Now >= _cTemplate.dtNext)
                    {
                        int nBuild;
                        XmlNode cResult, cXN, cXNChild;
                        XmlAttribute cXA;
                        XmlNode[] aItems;
                        XmlDocument cXmlDocument = new XmlDocument();
                        cXmlDocument.LoadXml((new System.Net.WebClient() { Encoding = Encoding.GetEncoding("windows-1251") }).DownloadString("http://www.cbr.ru/scripts/XML_daily.asp"));
                        nBuild = cXmlDocument.InnerText.GetHashCode();
                        if (_cTemplate.nBuild != nBuild)
                        {
                            aItems = cXmlDocument.NodesGet("ValCurs/Valute", false); //[Nominal=1]
                            if (null != aItems)
                            {
                                _cTemplate.nBuild = nBuild;
                                cXmlDocument = new XmlDocument();
                                cResult = cXmlDocument.CreateNode(XmlNodeType.Element, "result", null);
                                foreach (XmlNode cXNCurrency in aItems)
                                {
                                    cXN = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);

                                    cXNChild = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
                                    cXA = cXmlDocument.CreateAttribute("id");
                                    cXA.Value = "code";
                                    cXNChild.Attributes.Append(cXA);
                                    cXNChild.InnerText = cXNCurrency.NodeGet("CharCode").InnerText;
                                    cXN.AppendChild(cXNChild);

                                    cXNChild = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
                                    cXA = cXmlDocument.CreateAttribute("id");
                                    cXA.Value = "value";
                                    cXNChild.Attributes.Append(cXA);
                                    cXNChild.InnerText = " " + cXNCurrency.NodeGet("Value").InnerText;
                                    cXN.AppendChild(cXNChild);

                                    cResult.AppendChild(cXN);
                                }
                                _cTemplate.cValue = cResult;
                                _cTemplate.dt = DateTime.Now;
                            }
                            else
                                (new Logger()).WriteWarning("can't get currencies");
                        }
                    }
                    return _cTemplate.cValue;
                }
            }
            public class Stock : Request
            {
				private Template _cTemplate = new Template() { tsInterval = TimeSpan.FromHours(6) };

                public override XmlNode this[byte nTemplateID, object oValue]
                {
                    get
                    {
                        if (0 != nTemplateID)
                            throw new Exception("unknown currency request template [" + nTemplateID + "]");
                        return Yahoo();
                    }
                }
                private XmlNode Yahoo()
                {
                    if (DateTime.Now >= _cTemplate.dtNext)
                    {
                        int nBuild;
                        XmlNode cResult, cXN, cXNChild;
                        XmlAttribute cXA;
                        XmlNode[] aItems;
                        XmlDocument cXmlDocument = new XmlDocument();
                        cXmlDocument.LoadXml((new System.Net.WebClient() { Encoding = Encoding.UTF8 }).DownloadString("https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20in%20(%22%5EIPSA%22%2C%22%5ESSEC%22%2C%22%5EFCHI%22%2C%22%5EHSI%22%2C%22%5EJKSE%22%2C%22%5EN225%22%2C%22%5EKLSE%22%2C%22%5EAEX%22%2C%22%5EKS11%22%2C%22%5ESSMI%22%2C%22%5ETNX%22%2C%22%5EIRX%22%2C%22%5ETYX%22%2C%22%5EFVX%22%2C%22%5EFTSE%22%2C%22%5EATX%22%2C%22ASX%22%2C%22BSE%22%2C%22%5ESTI%22%2C%22DOW%22%2C%22RTS.RS%22)%0A&diagnostics=false&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys"));
                        nBuild = cXmlDocument.InnerText.GetHashCode();
                        if (_cTemplate.nBuild != nBuild)
                        {
                            aItems = cXmlDocument.NodesGet("query/results/quote", false);
							string sValue;
                            if (null != aItems)
                            {
                                _cTemplate.nBuild = nBuild;
                                cXmlDocument = new XmlDocument();
                                cResult = cXmlDocument.CreateNode(XmlNodeType.Element, "result", null);
                                foreach (XmlNode cXNQuote in aItems)
                                {
									if ((sValue = cXNQuote.NodeGet("Open").InnerText).IsNullOrEmpty())
										continue;
                                    cXN = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);

                                    cXNChild = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
                                    cXA = cXmlDocument.CreateAttribute("id");
                                    cXA.Value = "ticker";
                                    cXNChild.Attributes.Append(cXA);
                                    cXNChild.InnerText = cXNQuote.AttributeValueGet("symbol").Replace("^", "");
                                    cXN.AppendChild(cXNChild);

                                    cXNChild = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);
                                    cXA = cXmlDocument.CreateAttribute("id");
                                    if(cXNQuote.NodeGet("Change").InnerText.StartsWith("-"))
                                        cXA.Value = "negative";
                                    else
                                        cXA.Value = "positive";
                                    cXNChild.Attributes.Append(cXA);
									cXNChild.InnerText = " " + sValue;

                                    cXN.AppendChild(cXNChild);

                                    cResult.AppendChild(cXN);
                                }
                                _cTemplate.cValue = cResult;
                                _cTemplate.dt = DateTime.Now;
                            }
                            else
                                (new Logger()).WriteWarning("can't get currencies");
                        }
                    }
                    return _cTemplate.cValue;
                }
            }
			public class Poll : Request
			{
				private Template _cTemplate = new Template() { tsInterval = TimeSpan.FromSeconds(5) };

				public override XmlNode this[byte nTemplateID, object oValue]
				{
					get
					{
                        switch (nTemplateID)
                        {
                            case 0:
                                return Zed((string)oValue);
                            case 1:
                                return Api((string)oValue);
                            default:
                                throw new Exception("unknown poll request template [" + nTemplateID + "]");
                        }
					}
				}
				private XmlNode Zed(string sName)
				{
					if (DateTime.Now >= _cTemplate.dtNext)
					{
						int nBuild;
						XmlNode cResult, cXN, cXNChild;
						XmlAttribute cXA;
						XmlNode cXNPoll;
						XmlDocument cXmlDocument = new XmlDocument();
						cXmlDocument.LoadXml((new System.Net.WebClient() { Encoding = Encoding.UTF8 }).DownloadString("http://tvscope2014.agregator.ru/out/out_votings_one_results.phtml?dtStart=01.04.2016&dtEnd=01.04.3016"));
						nBuild = cXmlDocument.InnerXml.GetHashCode();
						if (_cTemplate.nBuild != nBuild)
						{
							string sValue;
							if (null != (cXNPoll = cXmlDocument.NodeGet("votings/voting", false)))
							{
								_cTemplate.nBuild = nBuild;
								cXmlDocument = new XmlDocument();
								cResult = cXmlDocument.CreateNode(XmlNodeType.Element, "result", null);

								if (sName.ToLower() != cXNPoll.AttributeValueGet("name").ToLower())
									throw new Exception("specified poll does not exist");
								foreach (XmlNode cXNVariant in cXNPoll.NodesGet("variant", false))
								{
									cXN = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);

									cXA = cXmlDocument.CreateAttribute("name");
									cXA.Value = cXNVariant.AttributeValueGet("name");
									cXN.Attributes.Append(cXA);
									cXA = cXmlDocument.CreateAttribute("votes");
									cXA.Value = cXNVariant.AttributeValueGet("votes");
									cXN.Attributes.Append(cXA);

									cResult.AppendChild(cXN);
								}

								_cTemplate.cValue = cResult;
								_cTemplate.dt = DateTime.Now;
							}
							else
								(new Logger()).WriteWarning("can't get any poll");
						}
					}
					return _cTemplate.cValue;
				}
                private XmlNode Api(string sUrl)
                {
                    if (DateTime.Now >= _cTemplate.dtNext)
                    {
                        int nBuild, nIndxStart, nIndxItemStart = 0, nIndxItemEnd = 0, nIndxValueStart, nIndxValueEnd;
                        XmlNode cResult, cXN, cXNChild;
                        XmlAttribute cXA;
                        XmlNode cXNPoll;
                        XmlDocument cXmlDocument;

                        string sJson = (new System.Net.WebClient() { Encoding = Encoding.UTF8 }).DownloadString(sUrl);
                        if (sJson.IsNullOrEmpty())
                        {
                            throw new Exception("got empty json. url = " + sUrl);
                        }
                        if ((nIndxStart = sJson.IndexOf("results")) < 0)
                        {
                            throw new Exception("got empty json. url = " + sUrl);
                        }
                        //sJson = sJson.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
                        nBuild = sJson.GetHashCode();
                        if (true || _cTemplate.nBuild != nBuild)
                        {
                            string sValue;
                            if (!sJson.IsNullOrEmpty())
                            {
                                _cTemplate.nBuild = nBuild;
                                cXmlDocument = new XmlDocument();
                                cResult = cXmlDocument.CreateNode(XmlNodeType.Element, "result", null);

                                nIndxItemStart = sJson.IndexOf("{", nIndxStart);
                                while (nIndxItemStart > -1)
                                {
                                    nIndxItemEnd = sJson.IndexOf("}", nIndxItemStart);
                                    cXN = cXmlDocument.CreateNode(XmlNodeType.Element, "item", null);

                                    cXA = cXmlDocument.CreateAttribute("name");
                                    nIndxValueStart = sJson.IndexOf("\"artist\"", nIndxItemStart);
                                    nIndxValueStart = sJson.IndexOf(":", nIndxValueStart);
                                    nIndxValueStart = sJson.IndexOf("\"", nIndxValueStart);
                                    nIndxValueEnd = sJson.IndexOf("\",", nIndxValueStart);
                                    cXA.Value = sJson.Substring(nIndxValueStart + 1, nIndxValueEnd - nIndxValueStart - 1);
                                    cXA.Value = UnescapeIt(cXA.Value);
                                    //cXA.Value = System.Web.HttpUtility.UrlDecode(cXA.Value);
                                    cXN.Attributes.Append(cXA);
                                    cXA = cXmlDocument.CreateAttribute("votes");
                                    nIndxValueStart = sJson.IndexOf("\"votes\"", nIndxValueEnd);
                                    nIndxValueStart = sJson.IndexOf(":", nIndxValueStart);
                                    nIndxValueStart = sJson.IndexOf("\"", nIndxValueStart);
                                    nIndxValueEnd = sJson.IndexOf("\"", nIndxValueStart + 1);
                                    cXA.Value = sJson.Substring(nIndxValueStart + 1, nIndxValueEnd - nIndxValueStart - 1);
                                    cXN.Attributes.Append(cXA);

                                    cResult.AppendChild(cXN);
                                    nIndxItemStart = sJson.IndexOf("{", nIndxItemEnd);
                                }

                                _cTemplate.cValue = cResult;
                                _cTemplate.dt = DateTime.Now;
                            }
                            else
                                (new Logger()).WriteWarning("can't get any poll");
                        }
                    }
                    return _cTemplate.cValue;
                }
                public static string UnescapeIt(string str)
                {
                    var regex = new System.Text.RegularExpressions.Regex(@"(?<!\\)(?:\\u[0-9a-fA-F]{4}|\\U[0-9a-fA-F]{8})", System.Text.RegularExpressions.RegexOptions.Compiled);
                    return regex.Replace(str,
                        m =>
                        {
                            if (m.Value.IndexOf("\\U", StringComparison.Ordinal) > -1)
                                return char.ConvertFromUtf32(int.Parse(m.Value.Replace("\\U", ""), System.Globalization.NumberStyles.HexNumber));
                            return System.Text.RegularExpressions.Regex.Unescape(m.Value);
                        });
                }
            }

            public XmlNode this[byte nTemplateID]
            {
                get
                {
                    return this[nTemplateID, null];
                }
            }
            abstract public XmlNode this[byte nTemplateID, object oValue]
            {
                get;
            }
        }
        static private Dictionary<string, Request> _ahRequests;

        static Data()
        {
            _ahRequests = new Dictionary<string, Request>();
            _ahRequests.Add("news.yandex", new Request.News());
            _ahRequests.Add("weather.yandex", new Request.Weather());
            _ahRequests.Add("currency.cbr", new Request.Currency());
            _ahRequests.Add("stock.yahoo", new Request.Stock());
			_ahRequests.Add("polls.zed", new Request.Poll());
            _ahRequests.Add("polls.api", new Request.Poll());
        }

        static public XmlNode Get(string sRequest, byte nTemplateID)
        {
            return Get(sRequest, nTemplateID, null);
        }
        static public XmlNode Get(string sRequest, byte nTemplateID, object oValue)
        {
            if(!_ahRequests.ContainsKey(sRequest))
                throw new Exception("unknown data request [" + sRequest + "]");
            lock(_ahRequests[sRequest])
                return _ahRequests[sRequest][nTemplateID, oValue];
		}
        public class WeatherItem
        {
            public int nID;
            public string sCity;
            public string sTime;
            public string sTemperature;
            public string sIcon;
            public string sDetales;
            public string sRegion;
            private WeatherItem(XmlNode cXmlNode)
            {
                nID = cXmlNode.AttributeGet<int>("output");
                string sValue;
                foreach (XmlNode cXN in cXmlNode.SelectNodes("item"))
                {
                    sValue = cXN.FirstChild.Value.Trim();
                    switch (cXN.AttributeValueGet("id"))
                    {
                        case "POST_CITY":
                            sCity = sValue;
                            break;
                        case "POST_TIME":
                            sTime = sValue;
                            break;
                        case "POST_TEMPERATURE":
                            sTemperature = sValue;
                            break;
                        case "POST_ICON":
                            sIcon = sValue;
                            break;
                        case "POST_DETAILS":
                            sDetales = sValue;
                            break;
                        case "POST_REGION":
                            sRegion = sValue;
                            break;
                    }
                }
            }
            static public List<WeatherItem> LoadItems(XmlNode cXmlNode)
            {
                List<WeatherItem> aRetVal = new List<WeatherItem>();
                foreach (XmlNode cXN in cXmlNode.SelectNodes("item"))
                    aRetVal.Add(new WeatherItem(cXN));
                return aRetVal;
            }
        }
    }
}