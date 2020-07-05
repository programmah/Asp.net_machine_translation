using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace YorubaTranslation
{
    //interface MyListMethod
    //{
    //    Dictionary<string, string> getDataSet( string fileName)
    //    {
    //        return
    //    }
        
    
    //}
    public class Translator 
    {
        public Translator( string EnglishSentence)
        {
            this.EnglishSentence = EnglishSentence;    
        }
        string line;
        public string EnglishSentence; 
      public  Dictionary<string,string> getDataSet( string fileName)
        {
            Dictionary<string, string> wordAndTranslate = new Dictionary<string, string>();
            string Path = @"C:\Users\user\Documents\Visual Studio 2012\Projects\YorubaTranslation\YorubaTranslation\dataset\" + fileName;
             StreamReader rd = new StreamReader(Path);
             while ((line = rd.ReadLine()) != null)
             {
                 string[] eachWordSet = line.Split(new Char[]{','}, StringSplitOptions.RemoveEmptyEntries );
                 if(eachWordSet.Length > 1)
                     if (!wordAndTranslate.Keys.Contains(eachWordSet[0].ToLower()) )
                     { wordAndTranslate.Add(eachWordSet[0].ToLower(), eachWordSet[1].ToLower()); }
                 else if (eachWordSet.Length == 1)
                         wordAndTranslate.Add(eachWordSet[0].ToLower(), "");
                  else
                     { }

             }

             return wordAndTranslate;
        }

      public List<WordSet> disectWord(Dictionary<string, string> dataset)
      {
          List<WordSet> newDataset = new List<WordSet>();
          foreach (KeyValuePair<string, string> word in dataset)
          {
              WordSet ws = new WordSet();
                string [] engAndPos =  word.Key.Split(':');
                if (engAndPos.Length > 1)
                {
                    ws.EnglishText = engAndPos[0];
                    ws.PartOfSpeech = engAndPos[1];
                    ws.YorubaText = word.Value;
                }
                else
                {
                    ws.EnglishText = engAndPos[0];
                    ws.PartOfSpeech = "";
                    ws.YorubaText = word.Value;
                }
                newDataset.Add(ws);
          }

          return newDataset;
      }
      
        public string TranslateEachWord()
        {

            Dictionary<string, string> detWordPair = getDataSet("determiners.txt");
            Dictionary<string, string> verbWordPair = getDataSet("auxilliary verb.txt");
            Dictionary<string, string> pronounWordPair = getDataSet("pronoun1.txt");
          //  Dictionary<string, string> allWordPair = getDataSet("dictionaryNew.txt");
            Dictionary<string, string> allWordPair = getDataSet("dictionary.txt");
            List<WordSet> wordClass = disectWord(allWordPair);

            string[] englishWordRaw = this.EnglishSentence.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string[] englishWordSCV = trackSentenceStartWithContnousVerb(englishWordRaw);
            string englishWordTokenContin = trackContinuousVerb(englishWordSCV);
            string[] englishWordTokenPara = englishWordTokenContin.Split(' ');

            string[] englishWordTokenIS = isTracking(englishWordTokenPara); // translate "is" to equivalent yoruba
            string englishWordTokenPL = TrackPluralNouns(englishWordTokenIS);
            string[] englishWordDet = reOrderDeterminant(englishWordTokenPL.Split(' '));
           // string[] englishWordNounPhrase = reoderingWordEndingInNoun(englishWordDet);

            string[] englishWordPP = trackVerbInPastOrPatcipleForm(englishWordDet);
           string englishWordSV = trackSingularVerbs(englishWordPP);
           
           
            
           
            List<string> foundTranslate = new List<string>();
            string yorubaWord = null;
            List<int> determinantIndex = new List<int>() ;




            string[] englishWordTokenArr = englishWordSV.Split(' ');
           
            //string[] englishWordTokenPara = englishWordTokenContin.Split(' ');
            string[] englishWordTokenTo = checkForTO(englishWordTokenArr);
            string[] englishWordTokenHas = checkForHAS(englishWordTokenTo);
//            string englishWordTokenVerbWtExtrWrd = verbWithExtraWord(englishWordTokenHas);
            string[] englishWordToken = englishWordTokenHas;
            for (int k = 0; k < englishWordToken.Length; k++ )  // translation process
            {
                if (englishWordToken[k] != string.Empty)
                {
                    if (detWordPair.Keys.Contains(englishWordToken[k].ToLower())) // det
                    {
                        foundTranslate.Add(detWordPair[englishWordToken[k].ToLower()]);
                        determinantIndex.Add(k);
                        //  i++;
                    }
                    else if (pronounWordPair.Keys.Contains(englishWordToken[k].ToLower())) // pronoun
                    {
                        foundTranslate.Add(pronounWordPair[englishWordToken[k].ToLower()]);
                    }
                    else if (verbWordPair.Keys.Contains(englishWordToken[k].ToLower()))// aux verb
                    {
                        foundTranslate.Add(verbWordPair[englishWordToken[k].ToLower()]);
                    }
                    else if (wordClass.Where(w=>w.EnglishText == englishWordToken[k].ToLower()).Count()> 0) // lexi verb,noun,adjective
                    {

                     //   string[] foundWord = wordClass.Where(w => w.EnglishText == englishWordToken[k].ToLower()).Select(w =>w.YorubaText).ToArray();
                         var foundWord = wordClass.Where(w => w.EnglishText == englishWordToken[k].ToLower()).Select(w =>new{ w.YorubaText, w.PartOfSpeech});
                          if (foundWord.ToArray().Length > 1)
                          {
                              string flag = wordTapRule(englishWordToken, englishWordToken[k]);
                              if (flag == "S")
                              {
                                //  foundTranslate.Add(foundWord[0]);
                                  foundTranslate.Add(foundWord.Where(w => w.PartOfSpeech == "n").Select(w => w.YorubaText).Single());
                              }
                              else if (flag == "M")
                              {
                                  foundTranslate.Add(foundWord.Where(w => w.PartOfSpeech == "v").Select(w => w.YorubaText).Single());
                                  //  foundTranslate.Add(foundWord[0]);
                              }
                              else
                              {
                                  foundTranslate.Add(foundWord.ToArray()[1].YorubaText);
                                 // foundTranslate.Add(foundWord[1]); 
                              }
                          }
                          else
                          {
                              foundTranslate.Add(foundWord.ToArray()[0].YorubaText);

                             // foundTranslate.Add(foundWord[0]);
                          }
                     
                      
                    }
                    else
                    {
                        foundTranslate.Add(englishWordToken[k]);
                    }
                }
            }
       //  string [] reOrderedTranslation =  reOrderDeterminant(foundTranslate.ToArray(), determinantIndex.ToArray());
            foreach (string word in foundTranslate)
            {
                yorubaWord += word + ' ';
            }

            return yorubaWord;
        }


        public string[] reOrderDeterminant(string[] translatedWord)
        {

            Dictionary<string, string> detWordPair = getDataSet("determiners.txt");
            Dictionary<string, string> all_WordPair = getDataSet("dictionary.txt");
            List<WordSet> allWordPair = disectWord(all_WordPair);
            for (int i = 0; i < translatedWord.Length; i++ )
            {
                if (detWordPair.Keys.Contains(translatedWord[i].ToLower()) && (i + 2) < translatedWord.Length)
                {
                    if (allWordPair.Where(w => w.EnglishText == translatedWord[i + 1].ToLower() && w.PartOfSpeech == "adj").Count() > 0 && allWordPair.Where(f => f.EnglishText == translatedWord[i + 2].ToLower() && f.PartOfSpeech == "n").Count() > 0)
                    {
                        string temp = translatedWord[i + 2];
                        translatedWord[i + 2] = translatedWord[i];
                        translatedWord[i] = temp;
                    }
                    else
                    {
                        if (allWordPair.Where(w => w.EnglishText == translatedWord[i + 1].ToLower() && (w.PartOfSpeech == "n" || w.PartOfSpeech == "adj")).Count() > 0)
                        {
                            string temp = translatedWord[i + 1];
                            translatedWord[i + 1] = translatedWord[i];
                            translatedWord[i] = temp;
                        }
                    }
                }
                
                else if(detWordPair.Keys.Contains(translatedWord[i].ToLower()) && (i+1) < translatedWord.Length )
                  {
                      if (allWordPair.Where(w => w.EnglishText == translatedWord[i + 1].ToLower() && (w.PartOfSpeech == "n" || w.PartOfSpeech == "adj")).Count() > 0)
                      {
                          string temp = translatedWord[i + 1];
                          translatedWord[i + 1] = translatedWord[i];
                          translatedWord[i] = temp;
                      }
                     

                 }
            }     
          //  if (DeterminantIndex.Length > 0)
          //          {
          //              for (int i = 0; i < DeterminantIndex.Length; i++)
          //              {
          //                  if (!((DeterminantIndex[i] + 1) > translatedWord.Length - 1))
          //                  {
          //                      string temp = translatedWord[DeterminantIndex[i] + 1];
          //                      translatedWord[DeterminantIndex[i] + 1] = translatedWord[DeterminantIndex[i]];
          //                      translatedWord[DeterminantIndex[i]] = temp;
          //                  }
          //              }
          //          }

            return translatedWord;
        }

        public string[] isTracking(string[] englishWordToken)
        {
             Dictionary<string, string> pronounWordPair = getDataSet("pronoun1.txt");
            Dictionary<string, string> all_WordPair = getDataSet("dictionary.txt");
             List<WordSet>allWordPair =  disectWord(all_WordPair);
            Dictionary<string, string> detWordPair = getDataSet("determiners.txt");
          //   List<int> position = new List<int>();
            for (int i = 0; i < englishWordToken.Length; i++)
            {
                if ((i + 1) < englishWordToken.Length)
                {
                    if ((englishWordToken[i].ToLower() == "is" || englishWordToken[i].ToLower() == "are") && pronounWordPair.Keys.Contains(englishWordToken[i + 1].ToLower()))
                    {
                        englishWordToken[i] = "je";

                    }
                     else if ((englishWordToken[i].ToLower() == "is" || englishWordToken[i].ToLower() == "are") && !(detWordPair.Keys.Contains(englishWordToken[i + 1].ToLower()))) //  is=""
                    {
                          englishWordToken[i] = "";
                    }
                    else if ((englishWordToken[i].ToLower() == "is" || englishWordToken[i].ToLower() == "are") && (englishWordToken[i + 1].ToLower() != "a" && (englishWordToken[i + 1].ToLower() != "the"))) //  is=""
                    {
                           englishWordToken[i] = "wa";
                    }
                    else if ((englishWordToken[i].ToLower() == "is" || englishWordToken[i].ToLower() == "are") && (englishWordToken[i + 1].ToLower() == "a"))  // a after is = "je" and a=""
                    {
                        englishWordToken[i] = "je";
                        englishWordToken[i + 1] = "";
                    }

                    else if ((englishWordToken[i].ToLower() == "is" || englishWordToken[i].ToLower() == "are") && (englishWordToken[i + 1].ToLower() == "the"))  // a after is = "je" and a=""
                    {
                        englishWordToken[i] = "ni";

                    }
                    else
                    {// englishWordToken[i] = "wa";
                    }
                }
               // else
               // {
              //      return englishWordToken;    
             //   }
            }
            return englishWordToken;
        
        }

        public string TrackPluralNouns(string[] englishWords) // required
        {
            Dictionary<string, string> all_WordPair = getDataSet("dictionary.txt");
             List<WordSet>allWordPair =  disectWord(all_WordPair);
          //  List<string> returnedWord = new List<string>();
            int pox = -1;
            string newSentence = "";
            int signal = 0;

           
                for (int i = 0; i < englishWords.Length; i++)
                {
                    string singularWord = englishWords[i] ;
                    if (englishWords[i].ToLower().EndsWith("s") || englishWords[i].ToLower().EndsWith("es") || englishWords[i].ToLower().EndsWith("ies") || englishWords[i].ToLower().EndsWith("ves") )
                    {
                        if (englishWords[i].Length > 3)
                        {
                            string wordIES = englishWords[i].ToLower().Substring(0, englishWords[i].Length - 3) + "y";
                            string wordFE = englishWords[i].ToLower().Substring(0, englishWords[i].Length - 3) + "fe";
                            if (englishWords[i].Length > 3 && englishWords[i] != "")
                            {
                                if ((allWordPair.Where(W => W.EnglishText == englishWords[i].ToLower().Substring(0, englishWords[i].Length - 1) && W.PartOfSpeech == "n").Count() > 0))
                                {
                                    singularWord = englishWords[i].ToLower().Substring(0, englishWords[i].Length - 1);
                                    signal++;
                                }
                            }
                            else if ((allWordPair.Where(W => W.EnglishText == wordIES && W.PartOfSpeech == "n").Count() > 0))
                            {
                                singularWord = wordIES;
                                signal++;
                            }
                            else if ((allWordPair.Where(W => W.EnglishText == wordFE && W.PartOfSpeech == "n").Count() > 0))
                            {
                                singularWord = wordFE;
                                signal++;
                            }
                            else
                            {
                                newSentence += englishWords[i] + ' ';
                                //   singularWord = englishWords[i];
                                // signal++;
                            }

                            if (i == 0 && signal > 0) // checking positioning
                            {
                                newSentence = "awon" + ' ' + singularWord + ' ';
                            }
                            else if (i == 1 && signal > 0)
                            {
                                newSentence = "awon" + ' ' + englishWords[0] + ' ' + singularWord + ' ';
                                pox = 1;
                            }
                            else if( i > 1 && signal > 0)
                            {

                                newSentence += "awon" + ' ' + singularWord + ' ';

                            }
                            else
                                newSentence += singularWord + ' ';
                        }
                    }     
                else
                {
                    if (signal > 0)
                    {
                        if (englishWords[i].ToLower() == "are")
                            englishWords[i] = "";
                    }
                   
                        newSentence += englishWords[i] + ' ';
                }
            }// closing for-loop

                

            return newSentence;
        }

        public string[] trackSentenceStartWithContnousVerb(string[] wordSet)
        {
            Dictionary<string, string> all_WordPair = getDataSet("dictionary.txt");
            List<WordSet> allWordPair = disectWord(all_WordPair);
             
            if (wordSet[0].ToLower().EndsWith("ing"))
            {
                if (wordSet[0].Length > 4 && wordSet[0] !="")
                {
                    if (allWordPair.Where(w => w.EnglishText == wordSet[0]).Count() == 0)
                    {
                        string gerundWord = wordSet[0].Substring(0, wordSet[0].Length - 3).ToLower();
                        string gerundWordAddE = gerundWord + "e";
                        if (allWordPair.Where(w => w.EnglishText == gerundWord).Count() > 0)
                            wordSet[0] = gerundWord;
                        else if (allWordPair.Where(w => w.EnglishText == gerundWordAddE).Count() > 0)
                            wordSet[0] = gerundWordAddE;
                        
                    }
                }
            }
            return wordSet;
        }
        public string trackSingularVerbs(string[] wordSet)
        {
            Dictionary<string, string> all_WordPair = getDataSet("dictionary.txt");
            List<WordSet> allWordPair = disectWord(all_WordPair);
            List<string> allVerb = allWordPair.Where(v => v.PartOfSpeech == "v").Select(v=>v.EnglishText).ToList();
            string sentence = "";
            for (int i = 0; i < wordSet.Length; i++)
            {
                if (wordSet[i].Trim().Length > 2 && wordSet[i] != "")
                {

                    if (wordSet[i].ToLower().EndsWith("s"))
                    {
                        if (!allVerb.Contains(wordSet[i]))
                        {
                            string removedSletter = wordSet[i].Trim().ToLower().Substring(0, wordSet[i].Length - 1);
                            if (allVerb.Contains(removedSletter))
                            {
                                // wordSet[i] = removedSletter;
                                sentence += "maa n" + ' ' + removedSletter + ' ';
                            }
                            else
                            {
                                sentence += wordSet[i] + " ";
                            }

                        }


                    }

                    else
                    {
                        sentence += wordSet[i] + " ";
                    }
                }

                else
                {
                    sentence += wordSet[i] + " ";  //  just added for two words correction
                }
            }

         //   return wordSet;
            return sentence;
        }

        public string wordTapRule(string[] wordSet, string word)
        {
            int pivot = -1;
            string pivotWord = "";

            string flag = null;
            if(wordSet.Length > 2)
            {
                    if (wordSet.Length % 2 == 0)
                    {
                        pivot = (wordSet.Length / 2 ) -1;
                        pivotWord = wordSet[pivot] ;
                    }
                    else
                    {
                        pivot = ((wordSet.Length + 1) / 2) - 1;
                        pivotWord = wordSet[pivot] ;
                    }
                    for (int i = 0; i < wordSet.Length; i++)
                    {
                       if(wordSet[i] == word)
                       {
                           if (i < pivot)
                               flag = "S";
                           else if (i > pivot)
                               flag = "O";
                           else
                               flag = "M"; // mid
                       }

                    }

                   
            }
            return flag;
        }
        public string[] reoderingWordEndingInNoun(string[] wordSet)
        {
            Dictionary<string, string> verbWordPair = getDataSet("auxilliary verb.txt");
            Dictionary<string, string> all_WordPair = getDataSet("dictionary.txt");
            List<WordSet> allWordPair = disectWord(all_WordPair);
            int verbPosition = -1;
            for (int i = 0; i < wordSet.Length; i++)
            {
                if (allWordPair.Where(w => w.EnglishText== wordSet[i] && w.PartOfSpeech=="v" ).Count() >0 || verbWordPair.Keys.Contains(wordSet[i]))
                {
                    verbPosition = i;
                    break;
                }
                
            }
            if (verbPosition > 2)
            {
                string temp = wordSet[verbPosition - 1];
                string[] tempArr = new string[verbPosition-1];
                for (int k =0; k < verbPosition-1; k++)
                {
                    tempArr[k] = wordSet[k];
                   
                }
                wordSet[0] = temp;
                for (int k = 1; k < verbPosition; k++)
                {
                  wordSet[k] = tempArr[k-1] ;

                }
            }
          //  string lastWord = wordSet[wordSet.Length - 1];
          return wordSet;
       //    return verbPosition.ToString();
        }
        public string[] trackVerbInPastOrPatcipleForm(string[] wordSet)
        {
            Dictionary<string, string> all_WordPair = getDataSet("dictionary.txt");
            List<WordSet> allWordPair = disectWord(all_WordPair);
            for (int i = 0; i < wordSet.Length; i++)
            {
                if (allWordPair.Where(w => w.EnglishText == wordSet[i].ToLower()).Count() == 0)
                {
                    if (wordSet[i].ToLower().EndsWith("ed") || wordSet[i].ToLower().EndsWith("en"))
                    {
                        if (wordSet[i].Length > 4 && wordSet[i] != "")
                        {
                            string removedEDorEN = wordSet[i].ToLower().Substring(0, wordSet[i].Length - 2);
                            string removedNorD = wordSet[i].ToLower().Substring(0, wordSet[i].Length - 1);
                            string doubleLetterWd = wordSet[i].ToLower().Substring(0, wordSet[i].Length - 3);// eg control-> controlled
                            if (allWordPair.Where(w => w.EnglishText == removedEDorEN).Count() > 0)
                                wordSet[i] = removedEDorEN;
                            else if (allWordPair.Where(w => w.EnglishText == removedNorD).Count() > 0)
                                wordSet[i] = removedNorD;
                            else if (allWordPair.Where(w => w.EnglishText == doubleLetterWd).Count() > 0)
                                wordSet[i] = doubleLetterWd;
                        }
                    }
                }
            }

            return wordSet;
        }
        public string trackContinuousVerb(string [] wordSet) // required
        {
            Dictionary<string, string> all_WordPair = getDataSet("dictionary.txt");
            List<WordSet> allWordPair = disectWord(all_WordPair);
            string newSentence = "";
           //  string[] vowel = { "a", "e", "o", "i", "u" };
            for (int j = 0; j < wordSet.Length; j++)
            {
                if (wordSet[j].ToLower().EndsWith("ing"))
                {
                    if (wordSet[j].Length > 4 && wordSet[j] != "")
                    {
                        string ingRemovedWord = (wordSet[j].Substring(0, wordSet[j].Length - 3)).ToLower();
                        string ingRemovedWordPlueE = ingRemovedWord + "e";
                        if (allWordPair.Where(W => W.EnglishText.Trim() == ingRemovedWord.Trim() && W.PartOfSpeech == "v").Count() > 0)
                            newSentence += "n" + ' ' + ingRemovedWord + ' ';
                        else if (allWordPair.Where(W => W.EnglishText.Trim() == ingRemovedWordPlueE.Trim() && W.PartOfSpeech == "v").Count() > 0)
                            newSentence += "n" + ' ' + ingRemovedWordPlueE + ' ';
                        else
                            newSentence += "n" + ' ' + wordSet[j];
                    }
                }
                else
                {
                    newSentence += wordSet[j]+' ';
                }

            }
            return newSentence;
        }

        public string[] checkForHAS(string[] wordSet)
        {
            Dictionary<string, string> all_WordPair = getDataSet("dictionary.txt");
            List<WordSet> allWordPair = disectWord(all_WordPair);

            for(int i =0 ; i< wordSet.Length; i++)
            {
                if (wordSet.Length >= (i + 1))
                {
                    if (wordSet[i].ToLower() == "has" || wordSet[i].ToLower() == "have")
                    {

                        if ((allWordPair.Where(W => W.EnglishText == wordSet[i + 1].ToLower() && W.PartOfSpeech == "v").Count() > 0) || (getDataSet("auxilliary verb.txt").Keys.Contains(wordSet[i + 1])))
                            wordSet[i] = "ti";
                        else
                            wordSet[i] = "ni";
                       
                    }
                    
                }
            }
            return wordSet;
        
        }

        public string[] checkForTO(string[] wordSet)
        {
            for (int i = 0; i < wordSet.Length; i++)
            {
                if (wordSet.Length< (i + 1))
                {
                    if (wordSet[i+1].ToLower() == "to")
                    {
                        if ((wordSet[i].ToLower() =="have" )|| (wordSet[i].ToLower() == "has"))
                        { wordSet[i+1] = "lati"; }
                        else
                        { wordSet[i+1] = "si"; }
                    }
                }
            }
            return wordSet;      
        }
        public string verbWithExtraWord(string[] wordSet) // rewrite
        {
            string newSentence = "";
            for (int i = 0; i < wordSet.Length; i++)
            {
                
                    if (wordSet[i].ToLower() == "me" && i!= wordSet.Length-1)
                            if (wordSet[i + 1].ToLower() == "in" || wordSet[i + 1].ToLower() == "at")
                            {
                               newSentence += wordSet[i] + ' ' + "ni" + ' ';
                                i++;
                            }
                          else
                                newSentence += wordSet[i] + ' ' + "ni" + ' ';

                    else
                        newSentence += wordSet[i] + ' ';
               
            }
            return newSentence;
        }
    }
    public class WordSet
    {
        public string EnglishText { set; get; }
        public string PartOfSpeech { set; get; }
        public string YorubaText { set; get; }
    }

    public class AddWord
    {
        public AddWord(string EnglishWord, string Pos, string YorubaWord):this( EnglishWord, YorubaWord)
        {
            this.EnglishWord = EnglishWord;
            this.Pos = Pos;
            this.YorubaWord = YorubaWord;
        
        }
        public AddWord(string EnglishWord, string YorubaWord) { }
        
        public string EnglishWord { set; get; }
        public string Pos { set; get; }
        public string YorubaWord { set; get; }
        string Path = @"C:\Users\Administrator\Documents\Visual Studio 2010\Projects\YorubaTranslation\YorubaTranslation\dataset\";
        public void AddNewOtherWord()
        {
            string newPath = Path +"dictionary.txt";
            string appendText = this.EnglishWord+":"+this.Pos+","+ this.YorubaWord + Environment.NewLine;
            File.AppendAllText(newPath, appendText);


        }

        public void AddGroupedWord(string fileName)
        {
            string newPath = Path + fileName;
            string appendText = this.EnglishWord + "," + this.YorubaWord + Environment.NewLine;
            File.AppendAllText(newPath, appendText);
        }
       
    }
}