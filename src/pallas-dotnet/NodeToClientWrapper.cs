using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PallasDotnetN2c
{    
    public static class PallasDotnetN2c
    {
        public class RustException: Exception {
            public RustException(string message) : base(message) { }
        }

        public interface IOpaqueHandle: IEquatable<IOpaqueHandle>, IDisposable {}

        
        public struct NetworkMagic {
        }
        public struct Point {
            public ulong slot;
            public List<byte> hash;
        }
        public struct TransactionInput {
            public List<byte> id;
            public ulong index;
        }
        public struct Datum {
            public byte datumType;
            public List<byte> data;
        }
        public struct TransactionOutput {
            public List<byte> address;
            public Value amount;
            public UIntPtr index;
            public Datum datum;
            public List<byte> raw;
        }
        public struct Value {
            public ulong coin;
            public Dictionary<List<byte>,Dictionary<List<byte>,ulong>> multiAsset;
        }
        public struct NextResponse {
            public byte action;
            public Point tip;
            public List<byte> blockCbor;
        }
        public struct NodeToClientWrapper {
            public UIntPtr clientPtr;
            public string socketPath;
        }
        public struct PallasUtility {
        }
        public static ulong MainnetMagic(
        ) {
            return _FnMainnetMagic();
        }
        public static ulong TestnetMagic(
        ) {
            return _FnTestnetMagic();
        }
        public static ulong PreviewMagic(
        ) {
            return _FnPreviewMagic();
        }
        public static ulong PreProductionMagic(
        ) {
            return _FnPreProductionMagic();
        }
        public static NodeToClientWrapper Connect(
            string socketPath,
            ulong networkMagic
        ) {
            return (_FnConnect(_AllocStr(socketPath),networkMagic)).Decode();
        }
        public static List<List<byte>> GetUtxoByAddressCbor(
            NodeToClientWrapper clientWrapper,
            string address
        ) {
            return _FreeSlice<List<byte>, _RawSlice, List<List<byte>>>(_FnGetUtxoByAddressCbor(_StructNodeToClientWrapper.Encode(clientWrapper),_AllocStr(address)), 16, 8, _arg1 => _FreeSlice<byte, byte, List<byte>>(_arg1, 1, 1, _arg2 => _arg2));
        }
        public static Point GetTip(
            NodeToClientWrapper clientWrapper
        ) {
            return (_FnGetTip(_StructNodeToClientWrapper.Encode(clientWrapper))).Decode();
        }
        public static Point FindIntersect(
            NodeToClientWrapper clientWrapper,
            Point knownPoint
        ) {
            return _DecodeOption(_FnFindIntersect(_StructNodeToClientWrapper.Encode(clientWrapper),_StructPoint.Encode(knownPoint)), _arg3 => (_arg3).Decode());
        }
        public static NextResponse ChainSyncNext(
            NodeToClientWrapper clientWrapper
        ) {
            return (_FnChainSyncNext(_StructNodeToClientWrapper.Encode(clientWrapper))).Decode();
        }
        public static bool ChainSyncHasAgency(
            NodeToClientWrapper clientWrapper
        ) {
            return (_FnChainSyncHasAgency(_StructNodeToClientWrapper.Encode(clientWrapper)) != 0);
        }
        public static void Disconnect(
            NodeToClientWrapper clientWrapper
        ) {
            _FnDisconnect(_StructNodeToClientWrapper.Encode(clientWrapper));
        }
        public static string AddressBytesToBech32(
            IReadOnlyCollection<byte> addressBytes
        ) {
            return _FreeStr(_FnAddressBytesToBech32(_AllocSlice<byte, byte>(addressBytes, 1, 1, _arg4 => _arg4)));
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _StructNetworkMagic {
            public static _StructNetworkMagic Encode(NetworkMagic structArg) {
                return new _StructNetworkMagic {
                };
            }
            public NetworkMagic Decode() {
                return new NetworkMagic {
                };
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _StructPoint {
            public ulong slot;
            public _RawSlice hash;
            public static _StructPoint Encode(Point structArg) {
                return new _StructPoint {
                    slot = structArg.slot,
                    hash = _AllocSlice<byte, byte>(structArg.hash, 1, 1, _arg5 => _arg5)
                };
            }
            public Point Decode() {
                return new Point {
                    slot = this.slot,
                    hash = _FreeSlice<byte, byte, List<byte>>(this.hash, 1, 1, _arg6 => _arg6)
                };
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _StructTransactionInput {
            public _RawSlice id;
            public ulong index;
            public static _StructTransactionInput Encode(TransactionInput structArg) {
                return new _StructTransactionInput {
                    id = _AllocSlice<byte, byte>(structArg.id, 1, 1, _arg7 => _arg7),
                    index = structArg.index
                };
            }
            public TransactionInput Decode() {
                return new TransactionInput {
                    id = _FreeSlice<byte, byte, List<byte>>(this.id, 1, 1, _arg8 => _arg8),
                    index = this.index
                };
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _StructDatum {
            public byte datumType;
            public _RawTuple0 data;
            public static _StructDatum Encode(Datum structArg) {
                return new _StructDatum {
                    datumType = structArg.datumType,
                    data = _EncodeOption(structArg.data, _arg9 => _AllocSlice<byte, byte>(_arg9, 1, 1, _arg10 => _arg10))
                };
            }
            public Datum Decode() {
                return new Datum {
                    datumType = this.datumType,
                    data = _DecodeOption(this.data, _arg11 => _FreeSlice<byte, byte, List<byte>>(_arg11, 1, 1, _arg12 => _arg12))
                };
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _StructTransactionOutput {
            public _RawSlice address;
            public _StructValue amount;
            public UIntPtr index;
            public _RawTuple1 datum;
            public _RawSlice raw;
            public static _StructTransactionOutput Encode(TransactionOutput structArg) {
                return new _StructTransactionOutput {
                    address = _AllocSlice<byte, byte>(structArg.address, 1, 1, _arg13 => _arg13),
                    amount = _StructValue.Encode(structArg.amount),
                    index = structArg.index,
                    datum = _EncodeOption(structArg.datum, _arg14 => _StructDatum.Encode(_arg14)),
                    raw = _AllocSlice<byte, byte>(structArg.raw, 1, 1, _arg15 => _arg15)
                };
            }
            public TransactionOutput Decode() {
                return new TransactionOutput {
                    address = _FreeSlice<byte, byte, List<byte>>(this.address, 1, 1, _arg16 => _arg16),
                    amount = (this.amount).Decode(),
                    index = this.index,
                    datum = _DecodeOption(this.datum, _arg17 => (_arg17).Decode()),
                    raw = _FreeSlice<byte, byte, List<byte>>(this.raw, 1, 1, _arg18 => _arg18)
                };
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _StructValue {
            public ulong coin;
            public _RawSlice multiAsset;
            public static _StructValue Encode(Value structArg) {
                return new _StructValue {
                    coin = structArg.coin,
                    multiAsset = _AllocDict<List<byte>, Dictionary<List<byte>,ulong>, _RawTuple2>(structArg.multiAsset, 32, 8, _arg19 => ((Func<(List<byte>,Dictionary<List<byte>,ulong>), _RawTuple2>)(_arg20 => new _RawTuple2 { elem0 = _AllocSlice<byte, byte>(_arg20.Item1, 1, 1, _arg21 => _arg21),elem1 = _AllocDict<List<byte>, ulong, _RawTuple3>(_arg20.Item2, 24, 8, _arg22 => ((Func<(List<byte>,ulong), _RawTuple3>)(_arg23 => new _RawTuple3 { elem0 = _AllocSlice<byte, byte>(_arg23.Item1, 1, 1, _arg24 => _arg24),elem1 = _arg23.Item2 }))(_arg22)) }))(_arg19))
                };
            }
            public Value Decode() {
                return new Value {
                    coin = this.coin,
                    multiAsset = _FreeDict<List<byte>, Dictionary<List<byte>,ulong>, _RawTuple2, Dictionary<List<byte>, Dictionary<List<byte>,ulong>>>(this.multiAsset, 32, 8, _arg25 => ((Func<_RawTuple2, (List<byte>,Dictionary<List<byte>,ulong>)>)(_arg26 => (_FreeSlice<byte, byte, List<byte>>(_arg26.elem0, 1, 1, _arg27 => _arg27),_FreeDict<List<byte>, ulong, _RawTuple3, Dictionary<List<byte>, ulong>>(_arg26.elem1, 24, 8, _arg28 => ((Func<_RawTuple3, (List<byte>,ulong)>)(_arg29 => (_FreeSlice<byte, byte, List<byte>>(_arg29.elem0, 1, 1, _arg30 => _arg30),_arg29.elem1)))(_arg28)))))(_arg25))
                };
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _StructNextResponse {
            public byte action;
            public _RawTuple4 tip;
            public _RawTuple0 blockCbor;
            public static _StructNextResponse Encode(NextResponse structArg) {
                return new _StructNextResponse {
                    action = structArg.action,
                    tip = _EncodeOption(structArg.tip, _arg31 => _StructPoint.Encode(_arg31)),
                    blockCbor = _EncodeOption(structArg.blockCbor, _arg32 => _AllocSlice<byte, byte>(_arg32, 1, 1, _arg33 => _arg33))
                };
            }
            public NextResponse Decode() {
                return new NextResponse {
                    action = this.action,
                    tip = _DecodeOption(this.tip, _arg34 => (_arg34).Decode()),
                    blockCbor = _DecodeOption(this.blockCbor, _arg35 => _FreeSlice<byte, byte, List<byte>>(_arg35, 1, 1, _arg36 => _arg36))
                };
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _StructNodeToClientWrapper {
            public UIntPtr clientPtr;
            public _RawSlice socketPath;
            public static _StructNodeToClientWrapper Encode(NodeToClientWrapper structArg) {
                return new _StructNodeToClientWrapper {
                    clientPtr = structArg.clientPtr,
                    socketPath = _AllocStr(structArg.socketPath)
                };
            }
            public NodeToClientWrapper Decode() {
                return new NodeToClientWrapper {
                    clientPtr = this.clientPtr,
                    socketPath = _FreeStr(this.socketPath)
                };
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _StructPallasUtility {
            public static _StructPallasUtility Encode(PallasUtility structArg) {
                return new _StructPallasUtility {
                };
            }
            public PallasUtility Decode() {
                return new PallasUtility {
                };
            }
        }
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_mainnet_magic", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong _FnMainnetMagic(
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_testnet_magic", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong _FnTestnetMagic(
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_preview_magic", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong _FnPreviewMagic(
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_pre_production_magic", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong _FnPreProductionMagic(
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_connect", CallingConvention = CallingConvention.Cdecl)]
        private static extern _StructNodeToClientWrapper _FnConnect(
            _RawSlice socketPath,
            ulong networkMagic
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_get_utxo_by_address_cbor", CallingConvention = CallingConvention.Cdecl)]
        private static extern _RawSlice _FnGetUtxoByAddressCbor(
            _StructNodeToClientWrapper clientWrapper,
            _RawSlice address
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_get_tip", CallingConvention = CallingConvention.Cdecl)]
        private static extern _StructPoint _FnGetTip(
            _StructNodeToClientWrapper clientWrapper
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_find_intersect", CallingConvention = CallingConvention.Cdecl)]
        private static extern _RawTuple4 _FnFindIntersect(
            _StructNodeToClientWrapper clientWrapper,
            _StructPoint knownPoint
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_chain_sync_next", CallingConvention = CallingConvention.Cdecl)]
        private static extern _StructNextResponse _FnChainSyncNext(
            _StructNodeToClientWrapper clientWrapper
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_chain_sync_has_agency", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte _FnChainSyncHasAgency(
            _StructNodeToClientWrapper clientWrapper
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_disconnect", CallingConvention = CallingConvention.Cdecl)]
        private static extern void _FnDisconnect(
            _StructNodeToClientWrapper clientWrapper
        );
        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_export_address_bytes_to_bech32", CallingConvention = CallingConvention.Cdecl)]
        private static extern _RawSlice _FnAddressBytesToBech32(
            _RawSlice addressBytes
        );
        [StructLayout(LayoutKind.Sequential)]
        private struct _RawTuple0 {
            public _RawSlice elem0;
            public byte elem1;
        }
        private static _RawTuple0 _EncodeOption<T>(T arg, Func<T, _RawSlice> converter) {
            if (arg != null) {
                return new _RawTuple0 { elem0 = converter(arg), elem1 = 1 };
            } else {
                return new _RawTuple0 { elem0 = default(_RawSlice), elem1 = 0 };
            }
        }
        private static T _DecodeOption<T>(_RawTuple0 arg, Func<_RawSlice, T> converter) {
            if (arg.elem1 != 0) {
                return converter(arg.elem0);
            } else {
                return default(T);
            }
        }
        private static _RawTuple0 _EncodeResult(Action f) {
            try {
                f();
                return new _RawTuple0 { elem0 = default(_RawSlice), elem1 = 1 };
            } catch (Exception e) {
                return new _RawTuple0 { elem0 = _AllocStr(e.Message), elem1 = 0 };
            }
        }
        private static void _DecodeResult(_RawTuple0 arg) {
            if (arg.elem1 == 0) {
                throw new RustException(_FreeStr(arg.elem0));
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _RawTuple1 {
            public _StructDatum elem0;
            public byte elem1;
        }
        private static _RawTuple1 _EncodeOption<T>(T arg, Func<T, _StructDatum> converter) {
            if (arg != null) {
                return new _RawTuple1 { elem0 = converter(arg), elem1 = 1 };
            } else {
                return new _RawTuple1 { elem0 = default(_StructDatum), elem1 = 0 };
            }
        }
        private static T _DecodeOption<T>(_RawTuple1 arg, Func<_StructDatum, T> converter) {
            if (arg.elem1 != 0) {
                return converter(arg.elem0);
            } else {
                return default(T);
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _RawTuple2 {
            public _RawSlice elem0;
            public _RawSlice elem1;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _RawTuple3 {
            public _RawSlice elem0;
            public ulong elem1;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _RawTuple4 {
            public _StructPoint elem0;
            public byte elem1;
        }
        private static _RawTuple4 _EncodeOption<T>(T arg, Func<T, _StructPoint> converter) {
            if (arg != null) {
                return new _RawTuple4 { elem0 = converter(arg), elem1 = 1 };
            } else {
                return new _RawTuple4 { elem0 = default(_StructPoint), elem1 = 0 };
            }
        }
        private static T _DecodeOption<T>(_RawTuple4 arg, Func<_StructPoint, T> converter) {
            if (arg.elem1 != 0) {
                return converter(arg.elem0);
            } else {
                return default(T);
            }
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void _ManageDelegateDelegate(IntPtr ptr, int adjust);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void _DropOpaqueDelegate(IntPtr ptr);

        private static Dictionary<IntPtr, (int, Delegate, Delegate)> _ActiveDelegates = new Dictionary<IntPtr, (int, Delegate, Delegate)>();

        private static _ManageDelegateDelegate _manageDelegate = _ManageDelegate;
        private static IntPtr _manageDelegatePtr = Marshal.GetFunctionPointerForDelegate(_manageDelegate);

        private static void _ManageDelegate(IntPtr ptr, int adjust)
        {
            lock (_ActiveDelegates)
            {
                var item = _ActiveDelegates[ptr];
                item.Item1 += adjust;
                if (item.Item1 > 0)
                {
                    _ActiveDelegates[ptr] = item;
                }
                else
                {
                    _ActiveDelegates.Remove(ptr);
                }
            }
        }

        private static _RawDelegate _AllocDelegate(Delegate d, Delegate original)
        {
            var ptr = Marshal.GetFunctionPointerForDelegate(d);
            lock (_ActiveDelegates)
            {
                if (_ActiveDelegates.ContainsKey(ptr))
                {
                    var item = _ActiveDelegates[ptr];
                    item.Item1 += 1;
                    _ActiveDelegates[ptr] = item;
                } else
                {
                    _ActiveDelegates.Add(ptr, (1, d, original));
                }
            }
            return new _RawDelegate
            {
                call_fn = ptr,
                drop_fn = _manageDelegatePtr,
            };
        }

        private static Delegate _FreeDelegate(_RawDelegate d)
        {
            var ptr = d.call_fn;
            lock (_ActiveDelegates)
            {
                var item = _ActiveDelegates[ptr];
                item.Item1 -= 1;
                if (item.Item1 > 0)
                {
                    _ActiveDelegates[ptr] = item;
                }
                else
                {
                    _ActiveDelegates.Remove(ptr);
                }
                return item.Item3;
            }
        }

        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_alloc", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr _Alloc( UIntPtr size, UIntPtr align);

        [DllImport("pallas_dotnet_n2c", EntryPoint = "rnet_free", CallingConvention = CallingConvention.Cdecl)]
        private static extern void _Free(IntPtr ptr, UIntPtr size, UIntPtr align);

        [StructLayout(LayoutKind.Sequential)]
        private struct _RawSlice
        {
            public IntPtr ptr;
            public UIntPtr len;

            public static _RawSlice Alloc(UIntPtr len, int size, int align)
            {
                if (len == UIntPtr.Zero)
                {
                    return new _RawSlice {
                        ptr = (IntPtr)align,
                        len = UIntPtr.Zero,
                    };
                } else
                {
                    return new _RawSlice
                    {
                        ptr = _Alloc((UIntPtr)((UInt64)len * (UInt64)size), (UIntPtr)align),
                        len = len,
                    };
                }
            }

            public void Free(int size, int align)
            {
                if (len != UIntPtr.Zero)
                {
                    _Free(ptr, (UIntPtr)((UInt64)len * (UInt64)size), (UIntPtr)align);
                    ptr = (IntPtr)1;
                    len = UIntPtr.Zero;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct _RawOpaqueHandle
        {
            public IntPtr ptr;
            public IntPtr drop_fn;
            public ulong type_id;

            public void Drop()
            {
                if (ptr != IntPtr.Zero)
                {
                    var drop = Marshal.GetDelegateForFunctionPointer<_DropOpaqueDelegate>(drop_fn);
                    drop(ptr);
                    ptr = IntPtr.Zero;
                }
            }
        }

        private class _OpaqueHandle : IOpaqueHandle
        {
            private _RawOpaqueHandle inner;

            public _OpaqueHandle(_RawOpaqueHandle inner)
            {
                this.inner = inner;
            }

            public _RawOpaqueHandle ToInner(ulong type_id)
            {
                if (type_id != inner.type_id)
                {
                    throw new InvalidCastException("Opaque handle does not have the correct type");
                }
                return this.inner;
            }

            ~_OpaqueHandle()
            {
                inner.Drop();
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as _OpaqueHandle);
            }

            public bool Equals(IOpaqueHandle other)
            {
                var casted = other as _OpaqueHandle;
                return casted != null &&
                       inner.ptr == casted.inner.ptr && inner.type_id == casted.inner.type_id;
            }

            public override int GetHashCode()
            {
                return inner.ptr.GetHashCode() + inner.type_id.GetHashCode();
            }

            public void Dispose()
            {
                inner.Drop();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct _RawDelegate
        {
            public IntPtr call_fn;
            public IntPtr drop_fn;
        }

        private static IntPtr _AllocBox<T>(T arg, int size, int align)
        {
            if (size > 0) {
                var ptr = _Alloc((UIntPtr)size, (UIntPtr)align);
                Marshal.StructureToPtr(arg, ptr, false);
                return ptr;
            } else {
                return (IntPtr)align;
            }
        }

        private static _RawSlice _AllocStr(string arg)
        {
            var nb = Encoding.UTF8.GetByteCount(arg);
            var slice = _RawSlice.Alloc((UIntPtr)nb, 1, 1);
            unsafe
            {
                fixed (char* firstChar = arg)
                {
                    nb = Encoding.UTF8.GetBytes(firstChar, arg.Length, (byte*)slice.ptr, nb);
                }
            }
            return slice;
        }

        private static _RawSlice _AllocSlice<T, U>(IReadOnlyCollection<T> collection, int size, int align, Func<T, U> converter) {
            var count = collection.Count;
            var slice = _RawSlice.Alloc((UIntPtr)count, size, align);
            var ptr = slice.ptr;
            foreach (var item in collection) {
                Marshal.StructureToPtr(converter(item), ptr, false);
                ptr = (IntPtr)(ptr.ToInt64() + (long)size);
            }
            return slice;
        }

        private static _RawSlice _AllocDict<TKey, TValue, U>(IReadOnlyDictionary<TKey, TValue> collection, int size, int align, Func<(TKey, TValue), U> converter) where U: unmanaged
        {
            var count = collection.Count;
            var slice = _RawSlice.Alloc((UIntPtr)count, size, align);
            var ptr = slice.ptr;
            foreach (var item in collection)
            {
                Marshal.StructureToPtr<U>(converter((item.Key, item.Value)), ptr, false);
                ptr = (IntPtr)(ptr.ToInt64() + (long)size);
            }
            return slice;
        }

        private static T _FreeBox<T>(IntPtr ptr, int size, int align)
        {
            var res = Marshal.PtrToStructure<T>(ptr);
            if (size > 0) {
                _Free(ptr, (UIntPtr)size, (UIntPtr)align);
            }
            return res;
        }

        private static String _FreeStr(_RawSlice arg)
        {
            unsafe
            {
                var res = Encoding.UTF8.GetString((byte*)arg.ptr, (int)arg.len);
                arg.Free(1, 1);
                return res;
            }
        }

        private static TList _FreeSlice<T, U, TList>(_RawSlice arg, int size, int align, Func<U, T> converter) where TList: ICollection<T>, new()
        {
            unsafe
            {
                var res = new TList();
                var ptr = arg.ptr;
                for (var i = 0; i < (int)arg.len; ++i) {
                    res.Add(converter(Marshal.PtrToStructure<U>(ptr)));
                    ptr = (IntPtr)(ptr.ToInt64() + (long)size);
                }
                arg.Free(size, align);
                return res;
            }
        }

        private static TDict _FreeDict<TKey, TValue, U, TDict>(_RawSlice arg, int size, int align, Func<U, (TKey, TValue)> converter) where U : unmanaged where TDict: IDictionary<TKey, TValue>, new()
        {
            unsafe
            {
                var res = new TDict();
                var ptr = arg.ptr;
                for (var i = 0; i < (int)arg.len; ++i)
                {
                    var item = converter(Marshal.PtrToStructure<U>(ptr));
                    res.Add(item.Item1, item.Item2);
                    ptr = (IntPtr)(ptr.ToInt64() + (long)size);
                }
                arg.Free(size, align);
                return res;
            }
        }
    }
}

