import java.lang.management.ManagementFactory;
import java.lang.reflect.Method;

public class App {
    public static void main(String args[]) {
        System.out.println("# Java app now running...");
        System.out.println("  - Java version: " + System.getProperty("java.version"));
        // print total cpus
        System.out.println("  - Available processors (cores): " + Runtime.getRuntime().availableProcessors());

        var osBean = ManagementFactory.getOperatingSystemMXBean();

        // Attempt to retrieve total physical memory size using reflection to maintain
        // compatibility
        // with non-com.sun implementations of OperatingSystemMXBean
        try {
            Method method = osBean.getClass().getMethod("getTotalPhysicalMemorySize");
            method.setAccessible(true);
            long totalMemorySize = (Long) method.invoke(osBean);

            System.out.println("  - OSBean.totalPhysicalMemory (in bytes): " + totalMemorySize);
        } catch (Exception e) {
            e.printStackTrace();
        }

        // print total memory from runtime
        System.out.println("  - Runtime.totalMemory (in bytes): " + Runtime.getRuntime().totalMemory());
        // print free memory
        System.out.println("  - Runtime.freeMemory (in bytes): " + Runtime.getRuntime().freeMemory());
        // print max memory
        System.out.println("  - Runtime.maxMemory (in bytes): " + Runtime.getRuntime().maxMemory());

        System.out.println("# Java app finished.");
    }
}