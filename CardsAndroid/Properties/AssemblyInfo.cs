using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Android.App;
using Android.Runtime;
using VKontakte;
using VKontakte.Utils;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle("CardsAndroid")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("${AuthorCopyright}")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion("1.0.0")]

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]
#if DEBUG
[Application(Debuggable = true)]
#else
[Application(Debuggable = false)]
#endif
public class MainApp : Application
{
    public MainApp(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();
        VKSdk.Initialize(this).WithPayments();

        // IMPORTANT!!! We need to get the fingerpint below and add it to console.
        //String[] fingerprints = VKUtil.GetCertificateFingerprint(this, this.PackageName);
    }
}
