using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBScript.errores
{
    public static class errores
    {
        public static List<error> lista = new List<error>();

        public static void add_error(String tipo, String descripcion, int fila, int columna) {
            error e = new error(tipo, descripcion, fila, columna);
            lista.Add(e);
        }

        public static void show() {
            foreach (error e in lista) {
                e.show();
            }
        }

        public static void clear() {
            lista = new List<error>();
        }
    }
}
