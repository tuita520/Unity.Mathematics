using NUnit.Framework;
using System;
using static Unity.Mathematics.math;

namespace Unity.Mathematics.Tests
{
    [TestFixture]
    class TestRandom
    {
        // Kolmogorov–Smirnov test on lambda assuming the ideal distribution is uniform [0, 1]
        private void ks_test(Func<double> func, int num_buckets = 256)
        {
            const int N = 2048;
            var histogram = new int[num_buckets];

            for (int i = 0; i < N; i++)
            {
                double x = func();
                Assert.GreaterOrEqual(x, 0.0);
                Assert.LessOrEqual(x, 1.0);
                int bucket = min((int)(x * num_buckets), num_buckets - 1);

                histogram[bucket]++;
            }

            double largest_delta = 0.0f;
            int accum = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                accum += histogram[i];
                double current = accum / (double)N;
                double target = (double)(i + 1) / histogram.Length;
                largest_delta = math.max(largest_delta, math.abs(current - target));
            }
            double d = 1.62762 / math.sqrt((double)N);   // significance: 0.01
            Assert.Less(largest_delta, d);
        }

        private void ks_test(Func<double2> func)
        {
            ks_test(() => func().x);
            ks_test(() => func().y);
        }

        // Pearson's product-moment coefficient
        private void r_test(Func<double2> func)
        {
            const int N = 4096;

            double2 sum = 0.0;
            var values = new double2[N]; 
            for(int i = 0; i < N; i++)
            {
                values[i] = func();
                sum += values[i];
            }

            double2 avg = sum / N;
            double var_a = 0.0;
            double var_b = 0.0;
            double cov = 0.0;
            for (int i = 0; i < N; i++)
            {
                double2 delta = values[i] - avg;
                var_a += delta.x * delta.x;
                var_b += delta.y * delta.y;
                cov += delta.x * delta.y;
            }

            double r = cov / sqrt(var_a * var_b);
            Assert.Less(abs(r), 0.05);
        }

        private float range_check01(float x)
        {
            Assert.GreaterOrEqual(x, 0.0f);
            Assert.Less(x, 1.0f);
            return x;
        }

        private double range_check01(double x)
        {
            Assert.GreaterOrEqual(x, 0.0);
            Assert.Less(x, 1.0);
            return x;
        }

        private int range_check(int x, int min, int max)
        {
            Assert.GreaterOrEqual(x, min);
            Assert.Less(x, max);
            return x;
        }

        private uint range_check(uint x, uint min, uint max)
        {
            Assert.GreaterOrEqual(x, min);
            Assert.Less(x, max);
            return x;
        }


        [Test]
        public void bool_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => rnd.NextBool() ? 0.75 : 0.25), 2);
        }

        [Test]
        public void bool2_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => rnd.NextBool2().x ? 0.75 : 0.25), 2);
            ks_test((() => rnd.NextBool2().y ? 0.75 : 0.25), 2);
        }

        [Test]
        public void bool2_independent()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test((() => select(double2(0.25), double2(0.75), rnd.NextBool2().xy)));
        }
        
        [Test]
        public void bool3_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => rnd.NextBool3().x ? 0.75 : 0.25), 2);
            ks_test((() => rnd.NextBool3().y ? 0.75 : 0.25), 2);
            ks_test((() => rnd.NextBool3().z ? 0.75 : 0.25), 2);
        }

        [Test]
        public void bool3_independent()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test((() => select(double2(0.25), double2(0.75), rnd.NextBool3().xy)));
            r_test((() => select(double2(0.25), double2(0.75), rnd.NextBool3().xz)));
            r_test((() => select(double2(0.25), double2(0.75), rnd.NextBool3().yz)));
        }

        [Test]
        public void bool4_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => rnd.NextBool4().x ? 0.75 : 0.25), 2);
            ks_test((() => rnd.NextBool4().y ? 0.75 : 0.25), 2);
            ks_test((() => rnd.NextBool4().z ? 0.75 : 0.25), 2);
            ks_test((() => rnd.NextBool4().w ? 0.75 : 0.25), 2);
        }

        [Test]
        public void bool4_independent()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test((() => select(double2(0.25), double2(0.75), rnd.NextBool4().xy)));
            r_test((() => select(double2(0.25), double2(0.75), rnd.NextBool4().xz)));
            r_test((() => select(double2(0.25), double2(0.75), rnd.NextBool4().xw)));
            r_test((() => select(double2(0.25), double2(0.75), rnd.NextBool4().yz)));
            r_test((() => select(double2(0.25), double2(0.75), rnd.NextBool4().yw)));
            r_test((() => select(double2(0.25), double2(0.75), rnd.NextBool4().zw)));
        }

        [Test]
        public void int_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextInt() & 255u) + 0.5) / 256.0), 256);
        }

        [Test]
        public void int_uniform_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => (((uint)rnd.NextInt() >> 24) + 0.5) / 256.0), 256);
        }

        [Test]
        public void int_uniform_max()
        {
            var rnd = new Random(0x6E624EB7u);
            int max = 17;
            ks_test((() => (range_check(rnd.NextInt(max), 0, max) + 0.5) / max), max);
        }

        [Test]
        public void int_uniform_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            int max = 2147483647;
            ks_test((() => (range_check(rnd.NextInt(max), 0, max) + 0.5) / max), 256);
        }

        [Test]
        public void int_uniform_min_max()
        {
            var rnd = new Random(0x6E624EB7u);
            int min = -7;
            int max = 17;
            int range = max - min;
            ks_test((() => (range_check(rnd.NextInt(min, max), min, max) + 0.5 - min) / range), range);
        }

        [Test]
        public void int_uniform_min_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            int min = -2147483648;
            int max = 2147483647;
            long range = (long)max - (long)min;
            ks_test((() => (range_check(rnd.NextInt(min, max), min, max) + 0.5 - min) / range), 256);
        }

        [Test]
        public void int2_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextInt2().x & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextInt2().y & 255u) + 0.5) / 256.0), 256);
        }

        [Test]
        public void int2_uniform_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => (((uint)rnd.NextInt2().x >> 24) + 0.5) / 256.0), 256);
            ks_test((() => (((uint)rnd.NextInt2().y >> 24) + 0.5) / 256.0), 256);
        }

        [Test]
        public void int2_uniform_max()
        {
            var rnd = new Random(0x6E624EB7u);
            int max = 2147483647;
            ks_test((() => (range_check(rnd.NextInt2(max).x, 0, max) + 0.5) / max), 256);
            ks_test((() => (range_check(rnd.NextInt2(max).y, 0, max) + 0.5) / max), 256);
        }

        [Test]
        public void int2_uniform_min_max()
        {
            var rnd = new Random(0x6E624EB7u);
            int2 min = int2(-7, 3);
            int2 max = int2(17, 14);
            int2 range = max - min;
            ks_test((() => (range_check(rnd.NextInt2(min, max).x, min.x, max.x) + 0.5 - min.x) / range.x), range.x);
            ks_test((() => (range_check(rnd.NextInt2(min, max).y, min.y, max.y) + 0.5 - min.y) / range.y), range.y);
        }

        [Test]
        public void int2_uniform_min_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            int min = -2147483648;
            int max = 2147483647;
            long range = (long)max - (long)min;
            ks_test((() => (range_check(rnd.NextInt2(min, max).x, min, max) + 0.5 - min) / range), 256);
            ks_test((() => (range_check(rnd.NextInt2(min, max).y, min, max) + 0.5 - min) / range), 256);
        }

        [Test]
        public void int2_independent_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => (rnd.NextInt2().xy & 255));
        }

        [Test]
        public void int2_independent_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => ((uint2)rnd.NextInt2().xy >> 24));
        }

        [Test]
        public void int3_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextInt3().x & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextInt3().y & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextInt3().z & 255u) + 0.5) / 256.0), 256);
        }

        [Test]
        public void int3_uniform_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => (((uint)rnd.NextInt3().x >> 24) + 0.5) / 256.0), 256);
            ks_test((() => (((uint)rnd.NextInt3().y >> 24) + 0.5) / 256.0), 256);
            ks_test((() => (((uint)rnd.NextInt3().z >> 24) + 0.5) / 256.0), 256);
        }

        [Test]
        public void int3_uniform_max()
        {
            var rnd = new Random(0x6E624EB7u);
            int3 max = int3(13, 17, 19);
            ks_test((() => ((uint)range_check(rnd.NextInt3(max).x, 0, max.x) + 0.5) / max.x), max.x);
            ks_test((() => ((uint)range_check(rnd.NextInt3(max).y, 0, max.y) + 0.5) / max.y), max.y);
            ks_test((() => ((uint)range_check(rnd.NextInt3(max).z, 0, max.z) + 0.5) / max.z), max.z);
        }

        [Test]
        public void int3_uniform_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            int max = 2147483647;
            ks_test((() => ((uint)range_check(rnd.NextInt3(max).x, 0, max) + 0.5) / max), 256);
            ks_test((() => ((uint)range_check(rnd.NextInt3(max).y, 0, max) + 0.5) / max), 256);
            ks_test((() => ((uint)range_check(rnd.NextInt3(max).z, 0, max) + 0.5) / max), 256);
        }

        [Test]
        public void int3_uniform_min_max()
        {
            var rnd = new Random(0x6E624EB7u);
            int3 min = int3(-7, 3, -10);
            int3 max = int3(17, 14, -3);
            int3 range = max - min;
            ks_test((() => (range_check(rnd.NextInt3(min, max).x, min.x, max.x) + 0.5 - min.x) / range.x), range.x);
            ks_test((() => (range_check(rnd.NextInt3(min, max).y, min.y, max.y) + 0.5 - min.y) / range.y), range.y);
            ks_test((() => (range_check(rnd.NextInt3(min, max).z, min.z, max.z) + 0.5 - min.z) / range.z), range.z);
        }

        [Test]
        public void int3_uniform_min_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            int min = -2147483648;
            int max = 2147483647;
            long range = (long)max - (long)min;
            ks_test((() => (range_check(rnd.NextInt3(min, max).x, min, max) + 0.5 - min) / range), 256);
            ks_test((() => (range_check(rnd.NextInt3(min, max).y, min, max) + 0.5 - min) / range), 256);
            ks_test((() => (range_check(rnd.NextInt3(min, max).z, min, max) + 0.5 - min) / range), 256);
        }

        [Test]
        public void int3_independent_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => (rnd.NextInt3().xy & 255));
            r_test(() => (rnd.NextInt3().xz & 255));
            r_test(() => (rnd.NextInt3().yz & 255));
        }

        [Test]
        public void int3_independent_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => ((uint2)rnd.NextInt3().xy >> 24));
            r_test(() => ((uint2)rnd.NextInt3().xz >> 24));
            r_test(() => ((uint2)rnd.NextInt3().yz >> 24));
        }

        [Test]
        public void int4_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextInt4().x & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextInt4().y & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextInt4().z & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextInt4().w & 255u) + 0.5) / 256.0), 256);
        }

        [Test]
        public void int4_uniform_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => (((uint)rnd.NextInt4().x >> 24) + 0.5) / 256.0), 256);
            ks_test((() => (((uint)rnd.NextInt4().y >> 24) + 0.5) / 256.0), 256);
            ks_test((() => (((uint)rnd.NextInt4().z >> 24) + 0.5) / 256.0), 256);
            ks_test((() => (((uint)rnd.NextInt4().w >> 24) + 0.5) / 256.0), 256);
        }

        [Test]
        public void int4_uniform_max()
        {
            var rnd = new Random(0x6E624EB7u);
            int4 max = int4(13, 17, 19, 23);
            ks_test((() => (range_check(rnd.NextInt4(max).x, 0, max.x) + 0.5) / max.x), max.x);
            ks_test((() => (range_check(rnd.NextInt4(max).y, 0, max.y) + 0.5) / max.y), max.y);
            ks_test((() => (range_check(rnd.NextInt4(max).z, 0, max.z) + 0.5) / max.z), max.z);
            ks_test((() => (range_check(rnd.NextInt4(max).w, 0, max.w) + 0.5) / max.w), max.w);
        }

        [Test]
        public void int4_uniform_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            int max = 2147483647;
            ks_test((() => (range_check(rnd.NextInt4(max).x, 0, max) + 0.5) / max), 256);
            ks_test((() => (range_check(rnd.NextInt4(max).y, 0, max) + 0.5) / max), 256);
            ks_test((() => (range_check(rnd.NextInt4(max).z, 0, max) + 0.5) / max), 256);
            ks_test((() => (range_check(rnd.NextInt4(max).w, 0, max) + 0.5) / max), 256);
        }

        [Test]
        public void int4_uniform_min_max()
        {
            var rnd = new Random(0x6E624EB7u);
            int4 min = int4(-7, 3, -10, 1);
            int4 max = int4(17, 14, -3, 111);
            int4 range = max - min;
            ks_test((() => (range_check(rnd.NextInt4(min, max).x, min.x, max.x) + 0.5 - min.x) / range.x), range.x);
            ks_test((() => (range_check(rnd.NextInt4(min, max).y, min.y, max.y) + 0.5 - min.y) / range.y), range.y);
            ks_test((() => (range_check(rnd.NextInt4(min, max).z, min.z, max.z) + 0.5 - min.z) / range.z), range.z);
            ks_test((() => (range_check(rnd.NextInt4(min, max).w, min.w, max.w) + 0.5 - min.w) / range.w), range.w);
        }

        [Test]
        public void int4_uniform_min_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            int min = -2147483648;
            int max = 2147483647;
            long range = (long)max - (long)min;
            ks_test((() => (range_check(rnd.NextInt4(min, max).x, min, max) + 0.5 - min) / range), 256);
            ks_test((() => (range_check(rnd.NextInt4(min, max).y, min, max) + 0.5 - min) / range), 256);
            ks_test((() => (range_check(rnd.NextInt4(min, max).z, min, max) + 0.5 - min) / range), 256);
            ks_test((() => (range_check(rnd.NextInt4(min, max).w, min, max) + 0.5 - min) / range), 256);
        }

        [Test]
        public void int4_independent_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => (rnd.NextUInt4().xy & 255));
            r_test(() => (rnd.NextUInt4().xz & 255));
            r_test(() => (rnd.NextUInt4().xw & 255));
            r_test(() => (rnd.NextUInt4().yz & 255));
            r_test(() => (rnd.NextUInt4().yw & 255));
            r_test(() => (rnd.NextUInt4().zw & 255));
        }

        [Test]
        public void int4_independent_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => ((uint2)rnd.NextUInt4().xy >> 24));
            r_test(() => ((uint2)rnd.NextUInt4().xz >> 24));
            r_test(() => ((uint2)rnd.NextUInt4().xw >> 24));
            r_test(() => ((uint2)rnd.NextUInt4().yz >> 24));
            r_test(() => ((uint2)rnd.NextUInt4().yw >> 24));
            r_test(() => ((uint2)rnd.NextUInt4().zw >> 24));
        }


        [Test]
        public void uint_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextUInt() & 255u) + 0.5) / 256.0), 256);
        }

        [Test]
        public void uint_uniform_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextUInt() >> 24) + 0.5) / 256.0), 256);
        }

        [Test]
        public void uint_uniform_max()
        {
            var rnd = new Random(0x6E624EB7u);
            uint max = 17;
            ks_test((() => (rnd.NextUInt(max) + 0.5) / max), (int)max);
        }

        [Test]
        public void uint_uniform_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            uint max = 0xFFFFFFFF;
            ks_test((() => (rnd.NextUInt(max) + 0.5) / max), 256);
        }

        [Test]
        public void uint_uniform_min_max()
        {
            var rnd = new Random(0x6E624EB7u);
            uint min = 3;
            uint max = 17;
            uint range = max - min;
            ks_test((() => (range_check(rnd.NextUInt(min, max), min, max) + 0.5 - min) / range), (int)range);
        }

        [Test]
        public void uint_uniform_min_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            uint max = 0xFFFFFFFF;
            ks_test((() => (range_check(rnd.NextUInt(0, max), 0, max) + 0.5) / max), 256);
        }

        [Test]
        public void uint2_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextUInt2().x & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt2().y & 255u) + 0.5) / 256.0), 256);
        }

        [Test]
        public void uint2_uniform_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextUInt2().x >> 24) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt2().y >> 24) + 0.5) / 256.0), 256);
        }

        [Test]
        public void uint2_uniform_max()
        {
            var rnd = new Random(0x6E624EB7u);
            uint2 max = uint2(13, 17);
            ks_test((() => (rnd.NextUInt2(max).x + 0.5) / max.x), (int)max.x);
            ks_test((() => (rnd.NextUInt2(max).y + 0.5) / max.y), (int)max.y);
        }

        [Test]
        public void uint2_uniform_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            uint max = 0xFFFFFFFF;
            ks_test((() => (rnd.NextUInt2(max).x + 0.5) / max), 256);
            ks_test((() => (rnd.NextUInt2(max).y + 0.5) / max), 256);
        }

        [Test]
        public void uint2_uniform_min_max()
        {
            var rnd = new Random(0x6E624EB7u);
            uint2 min = uint2(3, 101);
            uint2 max = uint2(17, 117);
            uint2 range = max - min;
            ks_test((() => (range_check(rnd.NextUInt2(min, max).x, min.x, max.x) + 0.5 - min.x) / range.x), (int)range.x);
            ks_test((() => (range_check(rnd.NextUInt2(min, max).y, min.y, max.y) + 0.5 - min.y) / range.y), (int)range.y);
        }

        [Test]
        public void uint2_uniform_min_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            uint max = 0xFFFFFFFF;
            ks_test((() => (range_check(rnd.NextUInt2(0, max).x, 0, max) + 0.5) / max), 256);
            ks_test((() => (range_check(rnd.NextUInt2(0, max).y, 0, max) + 0.5) / max), 256);
        }

        [Test]
        public void uint2_independent_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => (rnd.NextUInt2().xy & 255));
        }

        [Test]
        public void uint2_independent_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => (rnd.NextUInt2().xy >> 24));
        }

        [Test]
        public void uint3_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextUInt3().x & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt3().y & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt3().z & 255u) + 0.5) / 256.0), 256);
        }

        [Test]
        public void uint3_uniform_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextUInt3().x >> 24) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt3().y >> 24) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt3().z >> 24) + 0.5) / 256.0), 256);
        }

        [Test]
        public void uint3_uniform_max()
        {
            var rnd = new Random(0x6E624EB7u);
            uint3 max = uint3(13, 17, 19);
            ks_test((() => (rnd.NextUInt3(max).x + 0.5) / max.x), (int)max.x);
            ks_test((() => (rnd.NextUInt3(max).y + 0.5) / max.y), (int)max.y);
            ks_test((() => (rnd.NextUInt3(max).z + 0.5) / max.z), (int)max.z);
        }

        [Test]
        public void uint3_uniform_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            uint max = 0xFFFFFFFF;
            ks_test((() => (rnd.NextUInt3(max).x + 0.5) / max), 256);
            ks_test((() => (rnd.NextUInt3(max).y + 0.5) / max), 256);
            ks_test((() => (rnd.NextUInt3(max).z + 0.5) / max), 256);
        }

        [Test]
        public void uint3_uniform_min_max()
        {
            var rnd = new Random(0x6E624EB7u);
            uint3 min = uint3(3, 101, 0xFFFFFFF0);
            uint3 max = uint3(17, 117, 0xFFFFFFFF);
            uint3 range = max - min;
            ks_test((() => (range_check(rnd.NextUInt3(min, max).x, min.x, max.x) + 0.5 - min.x) / range.x), (int)range.x);
            ks_test((() => (range_check(rnd.NextUInt3(min, max).y, min.y, max.y) + 0.5 - min.y) / range.y), (int)range.y);
            ks_test((() => (range_check(rnd.NextUInt3(min, max).z, min.z, max.z) + 0.5 - min.z) / range.z), (int)range.z);
        }

        [Test]
        public void uint3_uniform_min_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            uint max = 0xFFFFFFFF;
            ks_test((() => (range_check(rnd.NextUInt3(0, max).x, 0, max) + 0.5) / max), 256);
            ks_test((() => (range_check(rnd.NextUInt3(0, max).y, 0, max) + 0.5) / max), 256);
            ks_test((() => (range_check(rnd.NextUInt3(0, max).z, 0, max) + 0.5) / max), 256);
        }

        [Test]
        public void uint3_independent_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => (rnd.NextUInt3().xy & 255));
            r_test(() => (rnd.NextUInt3().xz & 255));
            r_test(() => (rnd.NextUInt3().yz & 255));
        }

        [Test]
        public void uint3_independent_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => (rnd.NextUInt3().xy >> 24));
            r_test(() => (rnd.NextUInt3().xz >> 24));
            r_test(() => (rnd.NextUInt3().yz >> 24));
        }

        [Test]
        public void uint4_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextUInt4().x & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt4().y & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt4().z & 255u) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt4().w & 255u) + 0.5) / 256.0), 256);
        }

        [Test]
        public void uint4_uniform_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => ((rnd.NextUInt4().x >> 24) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt4().y >> 24) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt4().z >> 24) + 0.5) / 256.0), 256);
            ks_test((() => ((rnd.NextUInt4().w >> 24) + 0.5) / 256.0), 256);
        }

        [Test]
        public void uint4_uniform_max()
        {
            var rnd = new Random(0x6E624EB7u);
            uint4 max = uint4(13, 17, 19, 23);
            ks_test((() => (rnd.NextUInt4(max).x + 0.5) / max.x), (int)max.x);
            ks_test((() => (rnd.NextUInt4(max).y + 0.5) / max.y), (int)max.y);
            ks_test((() => (rnd.NextUInt4(max).z + 0.5) / max.z), (int)max.z);
            ks_test((() => (rnd.NextUInt4(max).w + 0.5) / max.w), (int)max.w);
        }

        [Test]
        public void uint4_uniform_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            uint max = 0xFFFFFFFF;
            ks_test((() => (rnd.NextUInt4(max).x + 0.5) / max), 256);
            ks_test((() => (rnd.NextUInt4(max).y + 0.5) / max), 256);
            ks_test((() => (rnd.NextUInt4(max).z + 0.5) / max), 256);
            ks_test((() => (rnd.NextUInt4(max).w + 0.5) / max), 256);
        }

        [Test]
        public void uint4_uniform_min_max()
        {
            var rnd = new Random(0x6E624EB7u);
            uint4 min = uint4(3, 101, 0xFFFFFFF0, 100);
            uint4 max = uint4(17, 117, 0xFFFFFFFF, 1000);
            uint4 range = max - min;
            ks_test((() => (range_check(rnd.NextUInt4(min, max).x, min.x, max.x) + 0.5 - min.x) / range.x), (int)range.x);
            ks_test((() => (range_check(rnd.NextUInt4(min, max).y, min.y, max.y) + 0.5 - min.y) / range.y), (int)range.y);
            ks_test((() => (range_check(rnd.NextUInt4(min, max).z, min.z, max.z) + 0.5 - min.z) / range.z), (int)range.z);
            ks_test((() => (range_check(rnd.NextUInt4(min, max).w, min.w, max.w) + 0.5 - min.w) / range.w), (int)range.w);
        }

        [Test]
        public void uint4_uniform_min_max_limit()
        {
            var rnd = new Random(0x6E624EB7u);
            uint max = 0xFFFFFFFF;
            ks_test((() => (range_check(rnd.NextUInt4(0, max).x, 0, max) + 0.5) / max), 256);
            ks_test((() => (range_check(rnd.NextUInt4(0, max).y, 0, max) + 0.5) / max), 256);
            ks_test((() => (range_check(rnd.NextUInt4(0, max).z, 0, max) + 0.5) / max), 256);
            ks_test((() => (range_check(rnd.NextUInt4(0, max).w, 0, max) + 0.5) / max), 256);
        }

        [Test]
        public void uint4_independent_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => (rnd.NextUInt4().xy & 255));
            r_test(() => (rnd.NextUInt4().xz & 255));
            r_test(() => (rnd.NextUInt4().xw & 255));
            r_test(() => (rnd.NextUInt4().yz & 255));
            r_test(() => (rnd.NextUInt4().yw & 255));
            r_test(() => (rnd.NextUInt4().zw & 255));
        }

        [Test]
        public void uint4_independent_high_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test(() => (rnd.NextUInt4().xy >> 24));
            r_test(() => (rnd.NextUInt4().xz >> 24));
            r_test(() => (rnd.NextUInt4().xw >> 24));
            r_test(() => (rnd.NextUInt4().yz >> 24));
            r_test(() => (rnd.NextUInt4().yw >> 24));
            r_test(() => (rnd.NextUInt4().zw >> 24));
        }

        [Test]
        public void float_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => range_check01(rnd.NextFloat())));
        }

        [Test]
        public void float_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => frac(rnd.NextFloat() * 65536.0f)));
        }

        [Test]
        public void float2_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => range_check01(rnd.NextFloat2().x)));
            ks_test((() => range_check01(rnd.NextFloat2().y)));
        }

        [Test]
        public void float2_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => frac(rnd.NextFloat2().x * 65536.0f)));
            ks_test((() => frac(rnd.NextFloat2().y * 65536.0f)));
        }

        [Test]
        public void float2_independent()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test((() => rnd.NextFloat2()));
        }

        [Test]
        public void float3_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => range_check01(rnd.NextFloat3().x)));
            ks_test((() => range_check01(rnd.NextFloat3().y)));
            ks_test((() => range_check01(rnd.NextFloat3().z)));
        }

        [Test]
        public void float3_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => frac(rnd.NextFloat3().x * 65536.0f)));
            ks_test((() => frac(rnd.NextFloat3().y * 65536.0f)));
            ks_test((() => frac(rnd.NextFloat3().z * 65536.0f)));
        }

        [Test]
        public void float3_independent()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test((() => rnd.NextFloat3().xy));
            r_test((() => rnd.NextFloat3().xz));
            r_test((() => rnd.NextFloat3().yz));
        }

        [Test]
        public void float4_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => range_check01(rnd.NextFloat4().x)));
            ks_test((() => range_check01(rnd.NextFloat4().y)));
            ks_test((() => range_check01(rnd.NextFloat4().z)));
            ks_test((() => range_check01(rnd.NextFloat4().w)));
        }

        [Test]
        public void float4_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => frac(rnd.NextFloat4().x * 65536.0f)));
            ks_test((() => frac(rnd.NextFloat4().y * 65536.0f)));
            ks_test((() => frac(rnd.NextFloat4().z * 65536.0f)));
            ks_test((() => frac(rnd.NextFloat4().w * 65536.0f)));
        }

        [Test]
        public void float4_independent()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test((() => rnd.NextFloat4().xy));
            r_test((() => rnd.NextFloat4().xz));
            r_test((() => rnd.NextFloat4().xw));
            r_test((() => rnd.NextFloat4().yz));
            r_test((() => rnd.NextFloat4().yw));
            r_test((() => rnd.NextFloat4().zw));
        }

        [Test]
        public void double_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => range_check01(rnd.NextDouble())));
        }

        [Test]
        public void double_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => frac(rnd.NextDouble() * 35184372088832.0)));
        }

        [Test]
        public void double2_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => range_check01(rnd.NextDouble2().x)));
            ks_test((() => range_check01(rnd.NextDouble2().y)));
        }

        [Test]
        public void double2_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => frac(rnd.NextDouble2().x * 35184372088832.0)));
            ks_test((() => frac(rnd.NextDouble2().y * 35184372088832.0)));
        }

        [Test]
        public void double2_independent()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test((() => rnd.NextDouble2()));
        }

        [Test]
        public void double3_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => range_check01(rnd.NextDouble3().x)));
            ks_test((() => range_check01(rnd.NextDouble3().y)));
            ks_test((() => range_check01(rnd.NextDouble3().z)));
        }

        [Test]
        public void double3_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => frac(rnd.NextDouble3().x * 35184372088832.0)));
            ks_test((() => frac(rnd.NextDouble3().y * 35184372088832.0)));
            ks_test((() => frac(rnd.NextDouble3().z * 35184372088832.0)));
        }

        [Test]
        public void double3_independent()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test((() => rnd.NextDouble3().xy));
            r_test((() => rnd.NextDouble3().xz));
            r_test((() => rnd.NextDouble3().yz));
        }

        [Test]
        public void double4_uniform()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => range_check01(rnd.NextDouble4().x)));
            ks_test((() => range_check01(rnd.NextDouble4().y)));
            ks_test((() => range_check01(rnd.NextDouble4().z)));
            ks_test((() => range_check01(rnd.NextDouble4().w)));
        }

        [Test]
        public void double4_uniform_low_bits()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test((() => frac(rnd.NextDouble4().x * 35184372088832.0)));
            ks_test((() => frac(rnd.NextDouble4().y * 35184372088832.0)));
            ks_test((() => frac(rnd.NextDouble4().z * 35184372088832.0)));
            ks_test((() => frac(rnd.NextDouble4().w * 35184372088832.0)));
        }

        [Test]
        public void double4_independent()
        {
            var rnd = new Random(0x6E624EB7u);
            r_test((() => rnd.NextDouble4().xy));
            r_test((() => rnd.NextDouble4().xz));
            r_test((() => rnd.NextDouble4().xw));
            r_test((() => rnd.NextDouble4().yz));
            r_test((() => rnd.NextDouble4().yw));
            r_test((() => rnd.NextDouble4().zw));
        }

        [Test]
        public void float2_direction()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test(() => {
                float2 dir = rnd.NextFloat2Direction();
                Assert.AreEqual(1.0f, length(dir), 0.001f);
                return atan2(dir.x, dir.y)/(2.0f*(float)PI) + 0.5f;
            });
        }

        [Test]
        public void double2_direction()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test(() => {
                double2 dir = rnd.NextFloat2Direction();
                Assert.AreEqual(1.0, length(dir), 0.000001);
                return atan2(dir.y, dir.x) / (2.0 * PI) + 0.5;
            });
        }


        [Test]
        public void float3_direction()
        {
            var rnd = new Random(0x6E624EB7u);
            ks_test(() =>
            {
                float3 dir = rnd.NextFloat3Direction();
                float r = length(dir);
                Assert.AreEqual(1.0f, r, 0.001f);

                float phi = atan2(dir.y, dir.x) / (2.0f * (float)PI) + 0.5f;
                float z = saturate(dir.z / r * 0.5f + 0.5f);
                return double2(phi, z);
            });
        }
    }
}
