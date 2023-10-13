using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Helper class for debugging
    /// </summary>
    static class Logger
    {
        private static readonly int DEBUG_MODE = 5;

        public static void Log(int priority, string message)
        {
            Log(message, priority);
        }

        public static void Log(string message, int priority = 0)
        {
            Print(message, priority, 0);
        }

        public static void LogWarning(int priority, string message)
        {
            LogWarning(message, priority);
        }

        public static void LogWarning(string message, int priority = 0)
        {
            Print(message, priority, 1);
        }

        public static void LogError(int priority, string message)
        {
            LogError(message, priority);
        }

        public static void LogError(string message, int priority = 0)
        {
            Print(message, priority, 2);
        }

        #region Special Logs

        /// <summary>
        /// Logs a 2D matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="priority"></param>
        public static void LogMatrix(int[,] matrix, int priority = 0)
        {
            string message = "";

            for(int j = 0; j < matrix.GetLength(1); j++)
            {
                for(int i = 0; i < matrix.GetLength(0); i++)
                {
                    message += matrix[i, j];

                    if (i < matrix.GetLength(0) - 1)
                        message += ", ";
                }

                if (j < matrix.GetLength(1) - 1)
                    message += "\n";
            }

            Print(message, priority, 0);
        }         
        
        /// <summary>
        /// Logs a 2D matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="priority"></param>
        public static void LogMatrix(float[,] matrix, int priority = 0)
        {
            string message = "";

            for(int j = 0; j < matrix.GetLength(1); j++)
            {
                for(int i = 0; i < matrix.GetLength(0); i++)
                {
                    message += matrix[i, j].ToString().Replace(',', '.');

                    if (i < matrix.GetLength(0) - 1)
                        message += ", ";
                }

                if (j < matrix.GetLength(1) - 1)
                    message += "\n";
            }

            Print(message, priority, 0);
        }

        /// <summary>
        /// Logs a 1D matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="priority"></param>
        public static void LogMatrix(int[] matrix, int rowLength, int priority = 0)
        {
            string message = "";

            for (int j = 0; j < matrix.Length / rowLength; j++)
            {
                for (int i = 0; i < rowLength; i++)
                {
                    message += matrix[i + j * rowLength];

                    if (i < matrix.GetLength(0) - 1)
                        message += ", ";
                }

                if (j < matrix.Length / rowLength - 1)
                    message += "\n";
            }

            Print(message, priority, 0);
        }

        #endregion

        /// <summary>
        /// Prints a message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="priority"></param>
        /// <param name="type"></param>
        private static void Print(string message, int priority, int type)
        {
            switch(type)
            {
                case 0:
                    if (priority <= DEBUG_MODE)
                        Debug.Log(message);
                    break;
                case 1:
                    if (priority <= DEBUG_MODE)
                        Debug.LogWarning(message);
                    break;
                case 2:
                    if (priority <= DEBUG_MODE)
                        Debug.LogError(message);
                    break;
            }
        }
    }
}
