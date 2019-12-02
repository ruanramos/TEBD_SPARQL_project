using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Web;

public class MyBehavior : MonoBehaviour
{
    //\n |
    public Text displayText;
    public InputField inputField;
    public Toggle togglePrefixes;
    public InputField newPrefix;
    public InputField newPrefixUri;
    public Dropdown dropdown;
    public Image image;
    public InputField player1;
    public InputField player2;


    private GameObject _hideBackgroundPanel;
    private GameObject _resultPanel;


    private Dictionary<string, string> _prefixes;
    private float _timer;

    private Dictionary<string, string> _exampleQueries;

    private void Awake()
    {
        _prefixes = new Dictionary<string, string>
        {
            {"ex", "<http://example.org/stuff/1.0/>"},
            {"rdf", "<http://www.w3.org/1999/02/22-rdf-syntax-ns#>"},
            {"rdfs", "<http://www.w3.org/2000/01/rdf-schema#>"},
            {"foaf", "<http://www.w3.org/2000/01/rdf-schema#>"},
            {"rel", "<http://www.perceive.net/schemas/relationship/>"},
            {"show", "<http://example.org/vocab/show/>"},
            {"xsd", "<http://www.w3.org/2001/XMLSchema#>"},
            {"owl", "<http://www.w3.org/2002/07/owl#>"},
            {"prop", "<http://dbpedia.org/property/>"},
            {"dbr", "<http://dbpedia.org/resource/>"},
            {"dbo", "<http://dbpedia.org/ontology/>"},
            {"dbp", "<http://dbpedia.org/property/>"},
            {"dbc", "<http://dbpedia.org/class/>"}
        };

        _hideBackgroundPanel = GameObject.Find("HideBackgroundPanel");
        _hideBackgroundPanel.SetActive(false);
        _resultPanel = GameObject.Find("ScrollResultPanel");
        _resultPanel.SetActive(false);


        _exampleQueries = new Dictionary<string, string>
        {
            {
                "People who were born in Berlin before 1900", "PREFIX dbo: <http://dbpedia.org/ontology/>\n" +
                                                              "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>\nPREFIX foaf: <http://xmlns.com/foaf/0.1/>\nPREFIX dbr: <http://dbpedia.org/resource/>\n" +
                                                              "SELECT ?name ?birth ?death ?person WHERE\n{ ?person dbo:birthPlace dbr:Berlin. ?person dbo:birthDate ?birth . ?person foaf:name ?name . ?person dbo:deathDate ?death ." +
                                                              "\nFILTER(?birth < \"1900 -01-01\" ^^ xsd:date) .\n} ORDER BY ?name"
            },
            {
                "Soccer players, who are born in a country with more than 10 million inhabitants, who played as goalkeeper for a club that has a stadium with more than 30.000 seats and the club country is different from the birth country",
                "PREFIX dbo: <http://dbpedia.org/ontology/>\nPREFIX dbp: <http://dbpedia.org/property/>\nSELECT distinct ?soccerplayer ?countryOfBirth ?team ?countryOfTeam ?stadiumcapacity\n" +
                "WHERE { ?soccerplayer a dbo:SoccerPlayer;\n dbo:position|dbp:position <http://dbpedia.org/resource/Goalkeeper_(association_football)> ;\n " +
                "dbo:birthPlace/dbo:country * ?countryOfBirth ;\n #dbo:number 13 ;\n dbo:team ?team . ?team dbo:capacity ?stadiumcapacity;\n" +
                " dbo:ground ?countryOfTeam . ?countryOfBirth a dbo:Country;\n dbo:populationTotal ?population .\n ?countryOfTeam a dbo:Country. \n" +
                "FILTER(?countryOfTeam != ?countryOfBirth)\nFILTER(?stadiumcapacity > 30000)\nFILTER(?population > 10000000)\n} ORDER BY ?soccerplayer"
            },
            {
                "The name of all Trilogies on DBpedia",
                "SELECT * WHERE { ?trilogy a <http://dbpedia.org/class/yago/Trilogy107985825> . }"
            },
            {
                "Find someone",
                "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\nPREFIX owl: <http://www.w3.org/2002/07/owl#>\nPREFIX foaf: <http://xmlns.com/foaf/0.1/>\n" +
                "PREFIX dbo: <http://dbpedia.org/ontology/>\nPREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>\nSELECT ?s ?pic WHERE {{?s rdfs:label \"Obama\"@en ; a owl:Thing . ?s foaf:depiction ?pic.}\n" +
                "UNION\n{?altName rdfs:label \"Obama\"@en ; dbo:wikiPageRedirects ?s. ?s foaf:depiction ?pic.}}"
            },
            {
                "Find punk rock band members and band names with picture of band",
                "PREFIX dbo: <http://dbpedia.org/ontology/>\nPREFIX dbp: <http://dbpedia.org/resource/>\nPREFIX foaf: <http://xmlns.com/foaf/0.1/>\n" +
                "SELECT ?name ?bandname ?pic WHERE { ?person foaf:name ?name .\n?band dbo:bandMember ?person .\n?band dbo:genre dbp:Punk_rock. ?band foaf:name ?bandname .\n ?band foaf:depiction ?pic .}"
            }
        };
    }

    private void Start()
    {
        //StartCoroutine(SimpleGetRequest());
        inputField.text =
            "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\nSELECT * WHERE { ?a ?b ?c . } LIMIT 100";
        inputField.ActivateInputField();
    }

    private void Update()
    {
        //textContainer.text = inputField.text;
        if (!Input.GetMouseButtonDown(1) || !_resultPanel.activeInHierarchy) return;
        _resultPanel.SetActive(false);
        _hideBackgroundPanel.SetActive(false);
    }

    public void ExampleQuery()
    {
        switch (dropdown.value)
        {
            case 0:
                inputField.text =
                    "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\nSELECT * WHERE { ?a ?b ?c . } LIMIT 100";
                displayText.text = "Default Query";
                break;
            case 1:
                inputField.text = _exampleQueries["People who were born in Berlin before 1900"];
                displayText.text = "People who were born in Berlin before 1900";
                break;
            case 2:
                inputField.text = _exampleQueries[
                    "Soccer players, who are born in a country with more than 10 million inhabitants, who played as goalkeeper for a club that has a stadium with more than 30.000 seats and the club country is different from the birth country"];
                displayText.text =
                    "Soccer players, who are born in a country with more than 10 million inhabitants, who played as goalkeeper for a club that has a stadium with more than 30.000 seats and the club country is different from the birth country";
                break;
            case 3:
                inputField.text = _exampleQueries["The name of all Trilogies on DBpedia"];
                displayText.text = "The name of all Trilogies on DBpedia";
                break;
            case 4:
                inputField.text = _exampleQueries["Find someone"];
                displayText.text = "Find someone";
                break;
            case 5:
                inputField.text = _exampleQueries["Find punk rock band members and band names with picture of band"];
                displayText.text = "Find punk rock band members and band names with picture of band";
                break;
        }
    }

    private void AddPrefixesToQuery()
    {
        RemovePrefixes();
        foreach (var item in _prefixes)
        {
            inputField.text = "\nPREFIX " + item.Key + ": " + item.Value + inputField.text;
        }

        inputField.text = "\n" + inputField.text;
    }

    private void RemovePrefixes()
    {
        var pattern = @"\n.*\>";
        var replacement = "";
        inputField.text = Regex.Replace(inputField.text, pattern, replacement);
    }

    public void CreateNewPrefix()
    {
        var prefix = newPrefix.text.Replace(" ", "");
        var uri = newPrefixUri.text.Replace(" ", "");
        if (prefix == "" && uri == "") return;
        _prefixes.Add(prefix, "<" + uri + ">");
        newPrefix.text = "";
        newPrefixUri.text = "";
        if (togglePrefixes.isOn)
        {
            AddPrefixesToQuery();
        }

        displayText.text = "New prefix added";
    }

    private IEnumerator CheckIfValidQuery(string query)
    {
        const string validatorUrl = "http://sparql.org/validate/query";

        var wwwForm = new Dictionary<string, string>
        {
            {"query", query}, {"languageSyntax", "SPARQL"}, {"outputFormat", "sparql"}, {"linenumbers", "false"}
        };

        var www = UnityWebRequest.Post(validatorUrl, wwwForm);
        ChangeMessageColor(Color.green);
        displayText.text = "Processing\n" + _timer;
        StartCoroutine(Timer(www));
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            ChangeMessageColor(Color.red);
            displayText.text = www.error;
            Debug.LogError(www.error);
        }
        else
        {
            var responseText = www.downloadHandler.text;
            if (QueryHasError(responseText))
            {
                ChangeMessageColor(Color.red);
                displayText.text = "Error on query\n" +
                                   Regex.Match(www.downloadHandler.text,
                                       @"(Lexical .*\n|Encountered.*\n|Was expecting.*\n)").Groups[0].Value;
            }
            else
            {
                ChangeMessageColor(Color.green);
                displayText.text = "Valid Query. Processed in " + _timer + " seconds";
            }
        }
    }

    private static bool QueryHasError(string response)
    {
        return response.Contains("error:<");
    }

    public void ValidateQuery()
    {
        var query = inputField.text;
        StartCoroutine(CheckIfValidQuery(query));
    }

    public void SendQuery()
    {
        var query = inputField.text;
        StartCoroutine(SendQueryToServer(query));
    }

    private IEnumerator SendQueryToServer(string query)
    {
        var startTime = Time.time;

        var sparqlEndpoint = "https://dbpedia.org/sparql?" + "default-graph-uri=http%3A%2F%2Fdbpedia.org" + "&query=" +
                             HttpUtility.UrlEncode(query.Replace(' ', '+')).Replace("%2b", "+") + "&format=" +
                             HttpUtility.UrlEncode("text/html") +
                             "&CXML_redir_for_subjs=121&CXML_redir_for_hrefs=&timeout=30000&debug=on&run=+Run+Query+";

        var www = UnityWebRequest.Get(sparqlEndpoint);
        ChangeMessageColor(Color.green);
        displayText.text = "Processing\n" + _timer;
        StartCoroutine(Timer(www));
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            ChangeMessageColor(Color.red);
            displayText.text = www.downloadHandler.text.Split('\n')[0];
        }
        else
        {
            _hideBackgroundPanel.SetActive(true);
            _resultPanel.SetActive(true);
            var noTags = Regex.Replace(www.downloadHandler.text,
                @"(<th>|</th>|<td>|</td>|<tr>|</tr>|</pre>|<pre>|<table .*>|</table>)", "");
            _resultPanel.GetComponentInChildren<Text>().text = noTags;

            foreach (var item in (noTags.Split('\n')))
            {
                if (item.Contains("jpg"))
                {
                    StartCoroutine(GetTexture(item.Trim().Split('"')[1], startTime));
                }
                else
                {
                    ChangeMessageColor(Color.green);
                    displayText.text = "Finished Processing in " + (Time.time - startTime) + " seconds";
                }
            }
        }
    }

    public void ToggleAction()
    {
        if (togglePrefixes.isOn)
        {
            AddPrefixesToQuery();
        }
        else
        {
            RemovePrefixes();
        }
    }

    private void ChangeMessageColor(Color color)
    {
        displayText.color = color;
    }

    private IEnumerator Timer(UnityWebRequest www)
    {
        _timer = 0;
        while (!www.isDone)
        {
            _timer += Time.deltaTime;
            displayText.text = "Processing\n" + _timer;
            yield return null;
        }
    }

    private IEnumerator GetTexture(string uri, float startTime)
    {
        var www = UnityWebRequestTexture.GetTexture(uri);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            var myTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
            image.sprite = Sprite.Create(myTexture, new Rect(0.0f, 0.0f, myTexture.width, myTexture.height),
                new Vector2(0.5f, 0.5f), 100.0f);
        }

        ChangeMessageColor(Color.green);
        displayText.text = "Finished Processing in " + (Time.time - startTime) + " seconds";
    }
}