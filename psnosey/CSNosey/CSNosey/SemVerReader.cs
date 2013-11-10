using System;

namespace CSNosey
{
    public class SemVerReader
    {
        private readonly string _semver;

        public SemVerReader(string semver)
        {
            _semver = semver;   
        }

        public Version GetVersion()
        {
            var parts = _semver.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var major = Int32.Parse(parts[1].Replace("major: ", string.Empty));
            var minor = Int32.Parse(parts[2].Replace("minor: ", string.Empty));
            var patch = Int32.Parse(parts[3].Replace("patch: ", string.Empty));

            return new Version(major, minor, patch);
        }
    }
}