//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.10
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


using System;
using System.Runtime.InteropServices;

namespace Noesis
{

public class ResourceKeyType : BaseComponent {
  internal new static ResourceKeyType CreateProxy(IntPtr cPtr, bool cMemoryOwn) {
    return new ResourceKeyType(cPtr, cMemoryOwn);
  }

  internal ResourceKeyType(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) {
  }

  internal static HandleRef getCPtr(ResourceKeyType obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  public Type Type {
    get {
      Noesis.Extend.NativeTypeInfo info = Noesis.Extend.GetNativeTypeInfo(GetTypeHelper());
      return info.Type;
    }
  }

  public ResourceKeyType() {
  }

  protected override IntPtr CreateCPtr(Type type, out bool registerExtend) {
    registerExtend = false;
    return NoesisGUI_PINVOKE.new_ResourceKeyType();
  }

  public IntPtr GetTypeHelper() {
    IntPtr ret = NoesisGUI_PINVOKE.ResourceKeyType_GetTypeHelper(swigCPtr);
    return ret;
  }

  new internal static IntPtr GetStaticType() {
    IntPtr ret = NoesisGUI_PINVOKE.ResourceKeyType_GetStaticType();
    return ret;
  }

}

}
