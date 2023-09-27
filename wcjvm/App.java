public class App {
    public static void main(String args[]) {
        System.out.println("Hello World");

        // print total cpus
        System.out.println("Available processors (cores): " + Runtime.getRuntime().availableProcessors());
        System.out.println("Total memory (bytes): " + Runtime.getRuntime().totalMemory());
    }
}