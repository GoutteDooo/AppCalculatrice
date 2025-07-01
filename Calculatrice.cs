using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AppCalculatrice
{
    /**
     * A chaque fois que l'utilisateur a entré un nombre, il doit entrer un opérateur, sinon une erreur est affichée, l'invitant à une nouvelle entrée
     * Lorsque l'utilisateur appuie sur "entrée", tout les éléments de la liste sont calculés dans une fonction "Calculer"
     * Tous les éléments de la liste sont ensuite supprimé
     * Et insère "resultat" dans la liste
     * La liste est affichée en permanence sous forme de string dans le format suivant :
     * n_o_n_o_n_... (n pour nombre, o pour opérateur, _ pour espace)
     * Et l'entrée utilisateur est toujours située un espace après
    */
    internal class Calculatrice
    {
        string[] caracteresValides = { "+", "-", "/", "*", "^", ".", "," };
        List<string> calculs = new();
        double dernierResultat = 0;
        string? input;


        /** 
         *  ----------------------
         *  ---- CALCULATRICE ----
         *  ----------------------
         * Se charge d'afficher les calculs effectués par l'utilisateur
         * Lorsque l'utilisateur appuie sur "entrée", le résultat est affiché
         */
        public void AfficherExpression()
        {
            while (true)
            {
                Console.Write("|------------- Calculatrice -------------|\n" +
                    $"|{new string(' ', 40)}|\n" +
                    $"|{new string(' ', 40)}|\n" +
                    $"|{new string(' ', 40)}|\n");
                // Afficher la ligne de l'expression
                AfficherLigneExpression(input, dernierResultat);
                // L'utilisateur doit commencer par entrer un nombre, sinon une erreur sera affichée
                input = Console.ReadLine();
                input = input.Trim(); // Retire les espaces inutiles

                // > "15  + 9  / 8 * 55.5" = 80
                // > "80 + 50 / 475"
                // S'il y'a eu un résultat précédemment, l'ajouter au tout début de la string de l'user
                input = dernierResultat.ToString() + input;

                //Console.WriteLine("dernierResultat:", input);

                calculs = ConvertirCalculs(input);

                input = $"{string.Join(" ", calculs)}"; // Pour l'affichage après le console.Clear

                double resultat = Calculer(calculs);
                calculs.Clear();
                dernierResultat = Math.Round(resultat, 7);

                Console.Clear();
            }
        }

        /**
         * Retourne le résultat d'une liste de calculs sous le format suivant :
         * [num,op,num,op,num...,num]
         */
        double Calculer(List<string> calculs)
        {
            // Puissances prioritaires
            AppliquerOperation(calculs, "^", (a, b) => Math.Pow(a, b));
            // Vérifie s'il y a des '*' et les calcules
            AppliquerOperation(calculs, "*", (a, b) => a * b);
            // Vérifie s'il y'a des '/' et les calcules
            AppliquerOperation(calculs, "/", (a, b) => a / b);
            // Vérifie s'il y'a des '-' et les calcules
            AppliquerOperation(calculs, "-", (a, b) => a - b);
            // Vérifie s'il y'a des '+' et les calcules
            AppliquerOperation(calculs, "+", (a, b) => a + b);

            return Convert.ToDouble(calculs[0]);
        }

        /**
         * Applique l'opération de la fonction "operation" passée en paramètre
         * Exemple :
         *      Func<double, double, double> operation reçoit (a, b) et retourne a * b
         */
        void AppliquerOperation(List<string> calculs, string operateur, Func<double, double, double> operation)
        {
            // Parcourir la liste et 
            for (int i = 0; i < calculs.Count; i++)
            {
                // Tant qu'on est pas au bout
                // S'il y'a l'opérateur en question qui apparaît,
                if (calculs[i] == operateur)
                {
                    // On calcule les deux nombres a gauche et a droite
                    // On les stocke dans une variable temp
                    double gauche = Convert.ToDouble(calculs[i - 1]);
                    double droite = Convert.ToDouble(calculs[i + 1]);
                    double resultat = operation(gauche, droite);
                    int iTemp = i - 1;
                    // On supprime les trois éléments de la liste
                    calculs.RemoveRange(iTemp, 3);
                    // Et on insère temp à l'index de '*' - 1 (iTemp)
                    calculs.Insert(iTemp, Convert.ToString(resultat));
                    i--;
                }
            }
        }

        /**
         * Converti la string d'opération passée en paramètre en une liste de string.
         * 
         * Exemple :
         *  operations = "1+2.5-8*4/5.25"
         *  
         *  retour : ["1.0", "+", "2.5", "-", "8.0", "*", "4.0", "/", "5.25"]
        */
        List<string> ConvertirCalculs(string expression)
        {
            // Plusieurs choses à vérifier
            // 1. Enlever tout les whitespaces inutiles
            expression = expression.Replace(" ", "");
            // Et, si l'expression commence par un "+" ou un "-", on ajoute un "0" au début de la liste
            if (expression[0] == '+' || expression[0] == '-')
                expression = '0' + expression;
            // TODO:  * et / a faire
            // 2. Vérifier qu'il n'y a pas de caractères bizarres
            foreach (char c in expression)
            {
                // Il faudra probablement faire un throw pour renvoyer une erreur
                if (!caracteresValides.Contains(c.ToString()) && !char.IsDigit(c))
                {
                    Console.WriteLine("ERREUR DANS L'ECRITURE DE L'OPERATION");
                    return new List<string>();
                }
            }

            List<string> calculsListe = new List<string>();
            var matches = Regex.Matches(expression, @"\d+|\+|\-|\*|\/|\^|\.|\,"); // Utilisation d'un regex pour "splitter" les opérateurs
            foreach (Match match in matches)
            {
                calculsListe.Add(match.ToString());
            }
            // Ici, on obtient une liste contenant l'expression entrée avec les opérateurs, mais les floatings ne sont pas encore bons
            // Exemple de liste : ["12",".","25","+","40"]
            // On a besoin de concaténer les floatings
            /**
             * La fonction ci-dessous permet de récupérer tout les "." Dans la liste, et de concaténer son gauche et son droit ensemble
             * Par exemple : ["12",".","25","+","40"] => ["12.25","+","40"]
             */
            AppliquerOperation(calculsListe, ".", (a, b) => a + b / Math.Pow(10, b.ToString().Length));
            AppliquerOperation(calculsListe, ",", (a, b) => a + b / Math.Pow(10, b.ToString().Length));
            // 3. Maintenant qu'on a une liste, on va d'abord vérifier qu'elle est conforme
            bool numState = true; // permet de connaître s'il doit y avoir un nombre ou non à l'index i
            for (int i = 0; i < calculsListe.Count; i++)
            {
                string op = calculsListe[i];
                // si entrée doit être un nombre
                if (numState)
                {
                    // Si ce n'est pas un nombre (float ou int)
                    if (!double.TryParse(op, out double nombre))
                    {
                        Console.Clear();
                        Console.WriteLine("ERREUR LORS DE L'ANALYSE DE LA LIGNE");
                        break;
                    }
                }
                else // Si entrée doit être un opérateur
                {
                    if (op.Length != 1 || !caracteresValides.Contains(op))
                    {
                        Console.Clear();
                        Console.WriteLine("ERREUR DE L'ANALYSE DE LA LIGNE");
                        break;
                    }
                }
                numState = !numState; // A la prochaine itération, on vérifie l'inverse de numState
            }
            // A partir d'ici, on est sensé avoir un tableau correct.

            return calculsListe;
        }

        void AfficherLigneExpression(string input, double dernierResultat)
        {
            Console.Write(
                $"|\t{(input == null ? new string(' ', 33) : input[0] == '0' ? input.Substring(1) + new string(' ', 34 - input.Length) : input + new string(' ', 33 - input.Length))}|\n" +
                $"|\t{(dernierResultat == 0 ? "" : "= " + dernierResultat)}");

        }
        void AfficherListe(List<string> liste)
        {
            Console.WriteLine($"{string.Join("/ ", liste.ToArray())}");
        }

    }
}
