const path = require("path");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const CopyWebpackPlugin = require("copy-webpack-plugin");
const devCerts = require("office-addin-dev-certs");

async function getHttpsOptions() {
  return devCerts.getHttpsServerOptions();
}

module.exports = async (_env, argv) => {
  const isProduction = argv.mode === "production";

  return {
    mode: argv.mode || "development",
    devtool: isProduction ? "source-map" : "eval-source-map",
    entry: {
      taskpane: "./src/taskpane/taskpane.tsx",
    },
    output: {
      clean: true,
      filename: "[name].js",
      path: path.resolve(__dirname, "dist"),
    },
    resolve: {
      extensions: [".ts", ".tsx", ".js"],
    },
    module: {
      rules: [
        {
          test: /\.tsx?$/,
          exclude: /node_modules/,
          use: {
            loader: "ts-loader",
            options: {
              transpileOnly: true,
              compilerOptions: { noEmit: false },
            },
          },
        },
        {
          test: /\.css$/,
          use: ["style-loader", "css-loader"],
        },
      ],
    },
    plugins: [
      new HtmlWebpackPlugin({
        filename: "taskpane.html",
        template: "./src/taskpane/taskpane.html",
        chunks: ["taskpane"],
      }),
      new CopyWebpackPlugin({
        patterns: [
          { from: "public/config.js", to: "config.js" },
          { from: "assets", to: "assets" },
          { from: "manifest.xml", to: "manifest.xml" },
        ],
      }),
    ],
    devServer: {
      port: 3000,
      server: {
        type: "https",
        options: isProduction ? {} : await getHttpsOptions(),
      },
      headers: {
        "Access-Control-Allow-Origin": "*",
      },
      hot: false,
    },
  };
};
