namespace ProtoWebServerLab.Common
{
    public class ConfigLoader
    {
        public enum eFileExtensionType
        {
            none = 0,
            json = 1
        }

        private static string? Find(in string filename, in eFileExtensionType type)
        {
            try
            {
                var root_path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName);
                if (string.IsNullOrEmpty(root_path))
                    return null;

                //var target_path = Directory.GetParent(root_path)?.Parent?.Parent;
                //if (null == target_path)
                //    return null;

                string filename_extension;
                switch (type)
                {
                    case eFileExtensionType.json:
                        filename_extension = $"{filename}.json";
                        break;
                    default:
                        return null;
                }

                var filepath = Path.Join(Path.Join(root_path, "data"), filename_extension);
                if (!File.Exists(filepath))
                    return null;

                return filepath;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Json
        private static T? ConvertJsonToObject<T>(in string file_path) where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(file_path))
                    return default(T);

                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(File.ReadAllText(file_path));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static T? LoadJson<T>(in string filename, in eFileExtensionType type) where T : class
        {
            try
            {
                var filepath = Find(filename, type);
                if (null == filepath)
                    return default(T);

                switch (type)
                {
                    case eFileExtensionType.json:
                        return ConvertJsonToObject<T>(filepath);
                    default:
                        return default(T);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
