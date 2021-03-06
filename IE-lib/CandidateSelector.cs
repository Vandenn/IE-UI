﻿using IE_lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IE_lib
{
    class CandidateSelector
    {
        public List<Candidate> performWhoCandidateSelection(List<Token> tokenizedArticle, String articleTitle)
        {
            List<Candidate> candidates = new List<Candidate>();
            List<Candidate> temporaryCandidates = new List<Candidate>();
            String[] startMarkers = new String[3] { "si",
                //"sina",
                //"kay",
                //"ni",
                "ang",
                "ng",
                /*"sa"*/};
            String[][] endMarkers = new String[3][] { new String[] { "na", "ng", ".", "bilang", "ang", "kamakalawa", "alyas", "at", "kay", ",", "sa", "makaraang", "mula"},
                /*new String[] { "at"},
                new String[] { "ng", "na"},
                new String[] { "ng", ",", "na", "ang"},*/
                new String[] { "na", "sa", "kay", "at", "ng", "makaraang", "para", "nang", "ang", "-LRB-", "mula"},
                new String[] { "ng", "ang", "si", "ang", ".", "para", "at", "na", "sa", "-LRB-", "mula"},
                /*new String[] { "sa", "na", "kaugnay", "ang", "upang", ",", ".", "-LRB-"}*/ }; 
            String[][] enderMarkers = new String[3][] { new String[] { "dahil", "kapag", "noong"},
                new String[] { "dahil", "kapag", "noong"},
                /*new String[] { "dahil", "kapag", "noong"},
                new String[] { "dahil", "kapag", "noong"},
                new String[] { "dahil", "kapag", "noong"},
                new String[] { "dahil", "kapag", "noong"},*/
                new String[] { "dahil", "kapag", "noong"}}; // Add all why start markers

            for (int i = 0; i < tokenizedArticle.Count; i++)
            {
                i = getCandidateByNer("PER", i, candidates, tokenizedArticle);
                i = getCandidateByNer("ORG", i, candidates, tokenizedArticle);
                getCandidateByMarkers(startMarkers, endMarkers, enderMarkers, i, temporaryCandidates, tokenizedArticle, true);

                if (tokenizedArticle[i].Sentence > 3)
                {
                    break;
                }
            }

            foreach (Candidate candidate in temporaryCandidates)
            {
                if (candidate.Value == null) continue;
                double candidateWeight = 0;
                int numWords = candidate.Value.Split(' ').Count();
                candidateWeight += 1 - (numWords / 5 + 1) * 0.2;
                if (candidate.Value.StartsWith("mga"))
                {
                    candidateWeight += 0.7;
                }
                if (articleTitle.Contains(candidate.Value))
                {
                    candidateWeight += 0.7;
                }

                bool found = false;
      
                for (int currentIndex = candidate.Position - 1; currentIndex < candidate.Position + candidate.Length - 1; currentIndex++)
                {
                    //Console.WriteLine(tokenizedArticle[currentIndex].PartOfSpeech);
                    if (tokenizedArticle[currentIndex].PartOfSpeech != null && tokenizedArticle[currentIndex].PartOfSpeech.StartsWith("V") || tokenizedArticle[currentIndex].PartOfSpeech.StartsWith("PR") || tokenizedArticle[currentIndex].PartOfSpeech.StartsWith("RB"))
                    {
                        Console.WriteLine(tokenizedArticle[currentIndex].PartOfSpeech);
                        candidateWeight = 0;
                        break;
                    }
                    /*if (tokenizedArticle[currentIndex].PartOfSpeech.StartsWith("N") && !found)
                    {
                        //Console.WriteLine("was here"+ candidateWeight);
                        candidateWeight += 0.3;
                        found = true;
                    }*/
                }


                if (candidateWeight >= 1)
                {
                    candidates.Add(candidate);
                }
            }
            //for (int i = 0; i < tokenizedArticle.Count; i++)
            //{
            //    i = getCandidateByPos("NNC", i, candidates, tokenizedArticle);
            //}

            for (int can = 0; can < candidates.Count; can++)
            {
                for (int a = 0; a < can; a++)
                {
                    if (candidates[can].Value != null && candidates[a].Value != null && candidates[can].Value.Equals(candidates[a].Value))
                    {
                        candidates.RemoveAt(can);
                        if (can > 0)
                        {
                            can--;
                        }
                        break;
                    }
                }
            }

            foreach (var candidate in candidates)
            {
                System.Console.WriteLine("WHO CANDIDATE " + candidate.Value);
            }

            return candidates;
        }

        public List<Candidate> performWhenCandidateSelection(List<Token> tokenizedArticle, String articleTitle)
        {
            List<Candidate> candidates = new List<Candidate>();
            String[] startMarkersExclusive = new String[] { "ang",
                "mula",
                "na",
                "noong",
                "nuong",
                "sa" };
            String[][] endMarkersExclusive = new String[][] { new String[] { "para"},
                new String[] { ",", "."},
                new String[] { "ay"},
                new String[] { ",", "."},
                new String[] { ",", "."},
                new String[] { "ay", "upang", ",", "."} };
            String[] startMarkersInclusive = new String[] { "kamakalawa",
                "kamakala-wa" };
            String[][] endMarkersInclusive = new String[][] { new String[] { "gabi", "umaga", "hapon" },
                new String[] { "gabi", "umaga", "hapon" } };
            String[] gazette = new String[] { "kahapon" };
            for (int i = 0; i < tokenizedArticle.Count; i++)
            {
                i = getCandidateByNer("DATE", i, candidates, tokenizedArticle);
                getCandidateByMarkers(startMarkersExclusive, endMarkersExclusive, null, i, candidates, tokenizedArticle, true);
                getCandidateByMarkers(startMarkersInclusive, endMarkersInclusive, null, i, candidates, tokenizedArticle, false);
                getCandidateByGazette(gazette, i, candidates, tokenizedArticle);

                if (tokenizedArticle[i].Sentence > 3)
                {
                    break;
                }
            }

            for (int can = 0; can < candidates.Count; can++)
            {
                for (int a = 0; a < can; a++)
                {
                    if (candidates[can].Value != null && candidates[can].Value.Equals(candidates[a].Value))
                    {
                        candidates.RemoveAt(can);
                        if (can > 0)
                        {
                            can--;
                        }
                        break;
                    }
                }
            }

            //foreach (var candidate in candidates)
            //{
            //    //System.Console.WriteLine("WHEN CANDIDATE " + candidate.Value);
            //}

            return candidates;
        }

        public List<Candidate> performWhereCandidateSelection(List<Token> tokenizedArticle, String articleTitle)
        {
            List<Candidate> candidates = new List<Candidate>();
            String[] startMarkers = new String[5] { "ang",
                "nasa",
                "noong",
                "nuong",
                "sa" };
            String[][] endMarkers = new String[5][] { new String[] { "ay", "."},
                new String[] { "para"},
                new String[] { "."},
                new String[] { "."},
                new String[] { "para", "noong", "nuong","sa","kamakalawa","kamakala-wa","."} };
            String[][] enderMarkers = new String[5][] { new String[] { },
                new String[] { },
                new String[] { "sabado", "hapon","umaga","gabi","miyerkules","lunes","martes","huwebes","linggo","biyernes","alas","oras"},
                new String[] { "sabado", "hapon","umaga","gabi","miyerkules","lunes","martes","huwebes","linggo","biyernes","alas","oras"},
                new String[] { "sabado", "hapon","umaga","gabi","miyerkules","lunes","martes","huwebes","linggo","biyernes","alas","oras"} };
            for (int i = 0; i < tokenizedArticle.Count; i++)
            {
                i = getCandidateByNer("LOC", i, candidates, tokenizedArticle);
                getCandidateByMarkers(startMarkers, endMarkers, enderMarkers, i, candidates, tokenizedArticle, true);

                if (tokenizedArticle[i].Sentence > 3)
                {
                    break;
                }
            }

            for (int can = 0; can < candidates.Count; can++)
            {
                for (int a = 0; a < can; a++)
                {
                    if (candidates[can].Value != null && candidates[can].Value.Equals(candidates[a].Value))
                    {
                        candidates.RemoveAt(can);
                        if (can > 0)
                        {
                            can--;
                        }
                        break;
                    }
                }
            }

            //foreach (var candidate in candidates)
            //{
            //    //System.Console.WriteLine("WHERE CANDIDATE " + candidate.Value);
            //}

            return candidates;
        }

        public List<List<Token>> performWhatCandidateSelection(List<Token> tokenizedArticle, String articleTitle)
        {
            int maxNumberOfCandidates = 3;
            List<List<Token>> candidates = new List<List<Token>>();
            List<List<Token>> segregatedArticle = tokenizedArticle
                .GroupBy(token => token.Sentence)
                .Select(tokenGroup => tokenGroup.ToList())
                .ToList();

            for (int nI = 0; nI < Math.Min(maxNumberOfCandidates, segregatedArticle.Count()); nI++)
            {
                candidates.Add(segregatedArticle[nI]);
            }

            return candidates;
        }

        public List<List<Token>> performWhyCandidateSelection(List<Token> tokenizedArticle, String articleTitle)
        {
            int maxNumberOfCandidates = 4;
            List<List<Token>> candidates = new List<List<Token>>();
            List<List<Token>> segregatedArticle = tokenizedArticle
                .GroupBy(token => token.Sentence)
                .Select(tokenGroup => tokenGroup.ToList())
                .ToList();

            for (int nI = 0; nI < Math.Min(maxNumberOfCandidates, segregatedArticle.Count()); nI++)
            {
                candidates.Add(segregatedArticle[nI]);
            }

            return candidates;
        }

        private int getCandidateByNer(String nerTag, int i, List<Candidate> candidates, List<Token> tokenizedArticle)
        {
            if (tokenizedArticle[i].NamedEntity.Equals(nerTag))
            {
                int startIndex = i;
                String strValue = tokenizedArticle[i].Value;
                int tempWs = tokenizedArticle[i].Frequency;

                while ((i + 1) < tokenizedArticle.Count && tokenizedArticle[i].NamedEntity == tokenizedArticle[i + 1].NamedEntity)
                {
                    i++;
                    strValue += " " + tokenizedArticle[i].Value;
                    if (tokenizedArticle[i].Frequency > tempWs)
                    {
                        tempWs = tokenizedArticle[i].Frequency;
                    }
                }

                int endIndex = i;

                var newToken = new Candidate(strValue, tokenizedArticle[startIndex].Position, tokenizedArticle[endIndex].Position - tokenizedArticle[startIndex].Position);
                newToken.Sentence = tokenizedArticle[i].Sentence; // candidate.token[0].sentence;
                newToken.NamedEntity = tokenizedArticle[i].NamedEntity; // candidate.token[0].NamedEntity;
                newToken.PartOfSpeech = tokenizedArticle[i].PartOfSpeech; // candidate.token[0].NamedEntity;
                newToken.Frequency = tempWs; // candidate.token[0].Frequency;
                candidates.Add(newToken);

                //System.Console.WriteLine("CANDIDATE BY NER [{0}]: {1} (Position {2})", nerTag, newToken.Value, newToken.Position);
            }
            return i;
        }

        private void getCandidateByMarkers(String[] startMarkers, String[][] endMarkers, String[][] enderMarkers, int i, List<Candidate> candidates, List<Token> tokenizedArticle, Boolean isExclusive)
        {

            for (int j = 0; j < startMarkers.Length; j++)
            {
                if (tokenizedArticle[i].Value.Equals(startMarkers[j], StringComparison.OrdinalIgnoreCase))
                {
                    if (isExclusive)
                    {
                        i++;
                    }
                    int startIndex = i;
                    int sentenceNumber = tokenizedArticle[i].Sentence;
                    String strValue = null;
                    String posValue = null;
                    int tempWs = 0;
                    Boolean flag = true;
                    Boolean endMarkerFound = false;
                    while (flag)
                    {
                        foreach (String markers in endMarkers[j])
                        {
                            if (tokenizedArticle[i].Value.Equals(markers))
                            {
                                endMarkerFound = true;
                                flag = false;
                                break;
                            }
                        }
                        if (enderMarkers != null)
                        {
                            foreach (String markers in enderMarkers[j])
                            {
                                if (tokenizedArticle[i].Value.Equals(markers, StringComparison.OrdinalIgnoreCase))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (tokenizedArticle[i].Sentence != sentenceNumber)
                        {
                            flag = false;
                        }
                        i++;
                        if (i >= tokenizedArticle.Count)
                        {
                            flag = false;
                        }
                    }

                    int endIndex;
                    if (isExclusive)
                    {
                        endIndex = i - 1;
                    }
                    else
                    {
                        endIndex = i;
                    }
                    if (endMarkerFound)
                    {
                        for (int k = startIndex; k < endIndex; k++)
                        {
                            if (strValue == null)
                            {
                                strValue = tokenizedArticle[k].Value;
                                posValue = tokenizedArticle[k].PartOfSpeech;
                            }
                            else
                            {
                                strValue += (tokenizedArticle[k].Value.Equals(",") || tokenizedArticle[k].Value.Equals(".") ? "" : " ") + tokenizedArticle[k].Value;
                                posValue += " " + tokenizedArticle[k].PartOfSpeech;
                            }

                            if (tokenizedArticle[k].Frequency > tempWs)
                            {
                                tempWs = tokenizedArticle[k].Frequency;
                            }
                        }
                        var newToken = new Candidate(strValue, tokenizedArticle[startIndex].Position, tokenizedArticle[endIndex].Position - tokenizedArticle[startIndex].Position);
                        newToken.Sentence = tokenizedArticle[startIndex].Sentence;
                        newToken.NamedEntity = tokenizedArticle[endIndex].NamedEntity;
                        newToken.PartOfSpeech = tokenizedArticle[endIndex].PartOfSpeech;
                        newToken.Frequency = tempWs;
                        candidates.Add(newToken);

                        //System.Console.WriteLine("CANDIDATE BY MARKERS: {0}"/*\n\t{1}*/, newToken.Value/*, posValue*/);
                    }
                    else
                    {
                        i = startIndex - 1;
                    }
                    j = startMarkers.Length;
                }
            }
        }

        private int getCandidateByPos(String posTag, int i, List<Candidate> candidates, List<Token> tokenizedArticle)
        {
            if (i < tokenizedArticle.Count && tokenizedArticle[i].PartOfSpeech != null && tokenizedArticle[i].PartOfSpeech.Equals(posTag))
            {
                int startIndex = i;
                String strValue = tokenizedArticle[i].Value;
                int tempWs = tokenizedArticle[i].Frequency;

                while ((i + 1) < tokenizedArticle.Count && tokenizedArticle[i].PartOfSpeech == tokenizedArticle[i + 1].PartOfSpeech)
                {
                    i++;
                    strValue += " " + tokenizedArticle[i].Value;
                    if (tokenizedArticle[i].Frequency > tempWs)
                    {
                        tempWs = tokenizedArticle[i].Frequency;
                    }
                }

                int endIndex = i;

                var newToken = new Candidate(strValue, tokenizedArticle[startIndex].Position, tokenizedArticle[endIndex].Position - tokenizedArticle[startIndex].Position);
                newToken.Sentence = tokenizedArticle[i].Sentence;
                newToken.NamedEntity = tokenizedArticle[i].NamedEntity;
                newToken.PartOfSpeech = tokenizedArticle[i].PartOfSpeech;
                newToken.Frequency = tempWs;
                candidates.Add(newToken);

                //System.Console.WriteLine("CANDIDATE BY POS [{0}]: {1} (Position {2})", posTag, newToken.Value, newToken.Position);
            }
            return i;
        }

        private void getCandidateByGazette(String[] gazette, int i, List<Candidate> candidates, List<Token> tokenizedArticle)
        {
            if (i < tokenizedArticle.Count && tokenizedArticle[i].Sentence <= 3)
            {
                if(gazette.Contains(tokenizedArticle[i].Value))
                {
                    var newToken = new Candidate(tokenizedArticle[i].Value, tokenizedArticle[i].Position, 1);
                    newToken.Sentence = tokenizedArticle[i].Sentence;
                    newToken.NamedEntity = tokenizedArticle[i].NamedEntity;
                    newToken.PartOfSpeech = tokenizedArticle[i].PartOfSpeech;
                    newToken.Frequency = tokenizedArticle[i].Frequency;
                    candidates.Add(newToken);

                    //System.Console.WriteLine("CANDIDATE BY GAZETTER: {0} (Position {1})", newToken.Value, newToken.Position);
                }
            }
        }
    }
}
